using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceMasterService
    : IAttendanceMasterService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlAttendanceMasterService(
        IOptions<ApplicationDbOptions> options)
    {
        _options = options.Value;
    }

    private NpgsqlDataSource CreateDataSource()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _options.Host,
            Port = _options.Port,
            Database = _options.Database,
            Username = _options.Username,
            Password = _options.Password,
            SslMode = SslMode.Disable,
            Timeout = 10,
            CommandTimeout = 15,
            ApplicationName = "BDIP Attendance Master"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<AttendanceMasterResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dataSource = CreateDataSource();

        var users = new Dictionary<Guid, AttendanceMasterUser>();

        // Master Attendance dimulai dari hasil import/discovery mesin.
        // User BDIP di-LEFT JOIN berdasarkan FingerID.
        //
        // Jika FingerID sudah ada di users:
        //   gunakan nama, NIP, dan status user BDIP.
        //
        // Jika FingerID belum ada:
        //   record tetap tampil dan nama sementara = FingerID.

        await using (var userCommand = dataSource.CreateCommand())
        {
            userCommand.CommandText = """
                SELECT
                    m.id,
                    u.id AS user_id,
                    COALESCE(NULLIF(TRIM(u.nip), ''), '') AS nip,
                    COALESCE(
                        NULLIF(TRIM(u.full_name), ''),
                        TRIM(m.finger_id)
                    ) AS full_name,
                    TRIM(m.finger_id) AS finger_id,
                    COALESCE(u.enabled, TRUE) AS user_enabled
                FROM public.attendance_finger_machine_users m
                LEFT JOIN public.users u
                    ON NULLIF(TRIM(u.finger_id), '') = TRIM(m.finger_id)
                ORDER BY
                    COALESCE(
                        NULLIF(TRIM(u.full_name), ''),
                        TRIM(m.finger_id)
                    );
                """;

            await using var reader =
                await userCommand.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var machineUserId = reader.GetGuid(0);

                // Matched user menggunakan users.id.
                // Unmatched user sementara menggunakan id record
                // attendance_finger_machine_users sampai operator Save.
                var userId = reader.IsDBNull(1)
                    ? machineUserId
                    : reader.GetGuid(1);

                users[userId] = new AttendanceMasterUser
                {
                    UserId = userId,
                    IsLinked = !reader.IsDBNull(1),
                    Nip = reader.IsDBNull(2)
                        ? string.Empty
                        : reader.GetString(2).Trim(),
                    FullName = reader.IsDBNull(3)
                        ? string.Empty
                        : reader.GetString(3).Trim(),
                    FingerId = reader.IsDBNull(4)
                        ? string.Empty
                        : reader.GetString(4).Trim(),
                    UserEnabled =
                        !reader.IsDBNull(5)
                        && reader.GetBoolean(5)
                };
            }
        }

        if (users.Count == 0)
        {
            return new AttendanceMasterResponse
            {
                Users = new List<AttendanceMasterUser>()
            };
        }

        // Fingerprint count.
        // Template saat ini terhubung ke users.id, sehingga matched
        // user dapat dihitung langsung melalui mapping FingerID.
        await using (var fingerprintCommand = dataSource.CreateCommand())
        {
            fingerprintCommand.CommandText = """
                SELECT
                    m.finger_id,
                    COUNT(t.id) AS fingerprint_count
                FROM public.attendance_finger_machine_users m
                LEFT JOIN public.attendance_finger_templates t
                    ON t.user_id = m.user_id
                GROUP BY m.finger_id;
                """;

            await using var reader =
                await fingerprintCommand.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var fingerId = reader.IsDBNull(0)
                    ? string.Empty
                    : reader.GetString(0).Trim();

                var fingerprintCount =
                    Convert.ToInt32(reader.GetInt64(1));

                var user = users.Values.FirstOrDefault(
                    x => string.Equals(
                        x.FingerId,
                        fingerId,
                        StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    user.FingerprintCount = fingerprintCount;
                }
            }
        }

        // Machine registration.
        // Jangan filter berdasarkan users.id karena unmatched
        // discovery record juga harus tetap muncul.
        await using (var machineCommand = dataSource.CreateCommand())
        {
            machineCommand.CommandText = """
                SELECT
                    m.finger_id,
                    f.code,
                    f.name,
                    TRUE AS registered,
                    m.enabled,
                    m.device_uid,
                    m.last_seen_at
                FROM public.attendance_finger_machine_users m
                INNER JOIN public.finger_machines f
                    ON f.id = m.finger_machine_id
                ORDER BY
                    TRIM(m.finger_id),
                    f.code;
                """;

            await using var reader =
                await machineCommand.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var fingerId = reader.IsDBNull(0)
                    ? string.Empty
                    : reader.GetString(0).Trim();

                var user = users.Values.FirstOrDefault(
                    x => string.Equals(
                        x.FingerId,
                        fingerId,
                        StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    continue;
                }

                user.Machines.Add(
                    new AttendanceMasterMachine
                    {
                        MachineCode = reader.GetString(1),
                        MachineName = reader.IsDBNull(2)
                            ? string.Empty
                            : reader.GetString(2),
                        Registered = true,
                        Enabled =
                            !reader.IsDBNull(4)
                            && reader.GetBoolean(4),
                        DeviceUid = reader.GetInt32(5),
                        LastSeenAt = reader.IsDBNull(6)
                            ? null
                            : reader.GetFieldValue<DateTime>(6)
                    });
            }
        }

        return new AttendanceMasterResponse
        {
            Users = users.Values
                .OrderBy(x => x.FullName)
                .ToList()
        };
    }

    public async Task SaveAsync(
        AttendanceMasterSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var fingerId = request.FingerId?.Trim() ?? string.Empty;
        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fingerId))
            throw new ArgumentException("FingerID wajib diisi.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Nama Lengkap wajib diisi.");

        await using var dataSource = CreateDataSource();
        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        Guid userId;

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var userKey = request.UserId.Trim();

            var resolveUserCommand = new NpgsqlCommand(
                """
                SELECT id
                FROM public.users
                WHERE username = @user_key
                   OR (id::text = @user_key)
                LIMIT 1
                """,
                connection,
                transaction);

            resolveUserCommand.Parameters.AddWithValue("user_key", userKey);

            var resolvedId =
                await resolveUserCommand.ExecuteScalarAsync(cancellationToken);

            if (resolvedId is not Guid resolvedUserId)
                throw new ArgumentException("User BDIP yang dipilih tidak ditemukan.");

            userId = resolvedUserId;

            var updateSelectedUserCommand = new NpgsqlCommand(
                """
                UPDATE public.users
                SET full_name = @full_name,
                    finger_id = @finger_id,
                    updated_at = NOW()
                WHERE id = @id
                """,
                connection,
                transaction);

            updateSelectedUserCommand.Parameters.AddWithValue(
                "full_name",
                fullName);

            updateSelectedUserCommand.Parameters.AddWithValue(
                "finger_id",
                fingerId);

            updateSelectedUserCommand.Parameters.AddWithValue(
                "id",
                userId);

            await updateSelectedUserCommand.ExecuteNonQueryAsync(
                cancellationToken);

            var oldUserCommand = new NpgsqlCommand(
                """
                SELECT DISTINCT user_id
                FROM public.attendance_finger_machine_users
                WHERE NULLIF(TRIM(finger_id), '') = @finger_id
                  AND user_id IS NOT NULL
                  AND user_id <> @user_id
                """,
                connection,
                transaction);

            oldUserCommand.Parameters.AddWithValue(
                "finger_id",
                fingerId);

            oldUserCommand.Parameters.AddWithValue(
                "user_id",
                userId);

            var oldUserIds = new List<Guid>();

            await using (var reader =
                await oldUserCommand.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    oldUserIds.Add(reader.GetGuid(0));
                }
            }

            foreach (var oldUserId in oldUserIds)
            {
                var deleteConflictingTemplatesCommand = new NpgsqlCommand(
                    """
                    DELETE FROM public.attendance_finger_templates old_t
                    WHERE old_t.user_id = @old_user_id
                      AND EXISTS (
                          SELECT 1
                          FROM public.attendance_finger_templates new_t
                          WHERE new_t.user_id = @new_user_id
                            AND new_t.fid = old_t.fid
                      )
                    """,
                    connection,
                    transaction);

                deleteConflictingTemplatesCommand.Parameters.AddWithValue(
                    "old_user_id",
                    oldUserId);

                deleteConflictingTemplatesCommand.Parameters.AddWithValue(
                    "new_user_id",
                    userId);

                await deleteConflictingTemplatesCommand.ExecuteNonQueryAsync(
                    cancellationToken);

                var moveTemplatesCommand = new NpgsqlCommand(
                    """
                    UPDATE public.attendance_finger_templates
                    SET user_id = @new_user_id,
                        updated_at = NOW()
                    WHERE user_id = @old_user_id
                    """,
                    connection,
                    transaction);

                moveTemplatesCommand.Parameters.AddWithValue(
                    "old_user_id",
                    oldUserId);

                moveTemplatesCommand.Parameters.AddWithValue(
                    "new_user_id",
                    userId);

                await moveTemplatesCommand.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            var linkCommand = new NpgsqlCommand(
                """
                UPDATE public.attendance_finger_machine_users
                SET user_id = @user_id,
                    updated_at = NOW()
                WHERE NULLIF(TRIM(finger_id), '') = @finger_id
                """,
                connection,
                transaction);

            linkCommand.Parameters.AddWithValue("user_id", userId);
            linkCommand.Parameters.AddWithValue("finger_id", fingerId);

            await linkCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            var findCommand = new NpgsqlCommand(
                """
                SELECT id
                FROM public.users
                WHERE NULLIF(TRIM(finger_id), '') = @finger_id
                LIMIT 1
                """,
                connection,
                transaction);

            findCommand.Parameters.AddWithValue(
                "finger_id",
                fingerId);

            var existingId =
                await findCommand.ExecuteScalarAsync(cancellationToken);

            if (existingId is Guid existingUserId)
            {
                userId = existingUserId;

                var updateCommand = new NpgsqlCommand(
                    """
                    UPDATE public.users
                    SET full_name = @full_name,
                        updated_at = NOW()
                    WHERE id = @id
                    """,
                    connection,
                    transaction);

                updateCommand.Parameters.AddWithValue(
                    "full_name",
                    fullName);

                updateCommand.Parameters.AddWithValue(
                    "id",
                    userId);

                await updateCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                userId = Guid.NewGuid();

                var insertCommand = new NpgsqlCommand(
                    """
                    INSERT INTO public.users
                        (id, username, full_name, enabled, finger_id, created_at, updated_at)
                    VALUES
                        (@id, @username, @full_name, TRUE, @finger_id, NOW(), NOW())
                    """,
                    connection,
                    transaction);

                insertCommand.Parameters.AddWithValue(
                    "id",
                    userId);

                insertCommand.Parameters.AddWithValue(
                    "username",
                    fingerId);

                insertCommand.Parameters.AddWithValue(
                    "full_name",
                    fullName);

                insertCommand.Parameters.AddWithValue(
                    "finger_id",
                    fingerId);

                await insertCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            var linkCommand = new NpgsqlCommand(
                """
                UPDATE public.attendance_finger_machine_users
                SET user_id = @user_id,
                    updated_at = NOW()
                WHERE NULLIF(TRIM(finger_id), '') = @finger_id
                """,
                connection,
                transaction);

            linkCommand.Parameters.AddWithValue("user_id", userId);
            linkCommand.Parameters.AddWithValue("finger_id", fingerId);

            await linkCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

}
