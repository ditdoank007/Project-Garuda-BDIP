using BDIP.Application.NAP;
using BDIP.Domain.NAP;
using BDIP.Persistence.PostgreSQL;
using BDIP.Application.Provisioning;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.NAP;

public sealed class PostgreSqlUserNapService : IUserNapService
{
    private readonly ApplicationDbOptions _options;

    private readonly IRadiusProvisioningService
    _radiusProvisioning;

    public PostgreSqlUserNapService(
        IOptions<ApplicationDbOptions> options,
        IRadiusProvisioningService radiusProvisioning)
    {
        _options = options.Value;
        _radiusProvisioning = radiusProvisioning;
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
            ApplicationName = "BDIP Backend"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<UserNap?> GetByUidAsync(string uid)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    uid,
                    download_kbps,
                    upload_kbps,
                    session_timeout,
                    idle_timeout,
                    created_at,
                    updated_at,
                    policy_id,
                    policy_code,
                    is_active
                FROM user_nap
                WHERE LOWER(uid)=LOWER(@uid);
                """);

        command.Parameters.AddWithValue("uid", uid);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return Map(reader);

        return null;
    }

    public async Task<IReadOnlyList<UserNap>> GetAllAsync()
    {
        var result = new List<UserNap>();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    uid,
                    download_kbps,
                    upload_kbps,
                    session_timeout,
                    idle_timeout,
                    created_at,
                    updated_at,
                    policy_id,
                    policy_code,
                    is_active
                FROM user_nap
                ORDER BY uid;
                """);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(Map(reader));
        }

        return result;
    }

public async Task<UserNap> UpdatePolicyAsync(
    string uid,
    Guid? policyId,
    string? policyCode)
    {
        await using var dataSource = CreateDataSource();

        // Cek apakah user sudah memiliki record NAP
        await using (var check = dataSource.CreateCommand(
            """
            SELECT COUNT(*)
            FROM user_nap
            WHERE LOWER(uid)=LOWER(@uid);
            """))
        {
            check.Parameters.AddWithValue("uid", uid);

            var exists =
                Convert.ToInt32(await check.ExecuteScalarAsync()) > 0;

            if (exists)
            {
                await using var update =
                    dataSource.CreateCommand(
                        """
                        UPDATE user_nap
                        SET
                            policy_id = @policy_id,
                            policy_code = @policy_code,
                            updated_at = NOW()
                        WHERE LOWER(uid)=LOWER(@uid);
                        """);

                update.Parameters.AddWithValue("uid", uid);
                update.Parameters.AddWithValue(
                    "policy_id",
                    (object?)policyId ?? DBNull.Value);

                update.Parameters.AddWithValue(
                    "policy_code",
                    (object?)policyCode ?? DBNull.Value);

                await update.ExecuteNonQueryAsync();
            }
            else
            {
                await using var insert =
                    dataSource.CreateCommand(
                        """
                        INSERT INTO user_nap
                        (
                            uid,
                            download_kbps,
                            upload_kbps,
                            session_timeout,
                            idle_timeout,
                            created_at,
                            updated_at,
                            policy_id,
                            policy_code,
                            is_active
                        )
                        VALUES
                        (
                            @uid,
                            0,
                            0,
                            0,
                            0,
                            NOW(),
                            NOW(),
                            @policy_id,
                            @policy_code,
                            TRUE
                        );
                        """);

                insert.Parameters.AddWithValue("uid", uid);
                insert.Parameters.AddWithValue(
                    "policy_id",
                    (object?)policyId ?? DBNull.Value);

                insert.Parameters.AddWithValue(
                    "policy_code",
                    (object?)policyCode ?? DBNull.Value);

                await insert.ExecuteNonQueryAsync();
            }
        }
        if (!string.IsNullOrWhiteSpace(policyCode))
        {
            await _radiusProvisioning
                .AssignUserGroupAsync(
                    uid,
                    policyCode);
        }
        else
        {
            await _radiusProvisioning
                .RemoveUserGroupAsync(uid);
        }

        return await GetByUidAsync(uid)
            ?? throw new KeyNotFoundException(
                $"User '{uid}' not found after upsert.");
    }

    private static UserNap Map(
        NpgsqlDataReader reader)
    {
        return new UserNap
        {
            Uid = reader.GetString(0),
            DownloadKbps = reader.GetInt32(1),
            UploadKbps = reader.GetInt32(2),
            SessionTimeout = reader.GetInt32(3),
            IdleTimeout = reader.GetInt32(4),
            CreatedAt = reader.GetFieldValue<DateTime>(5),
            UpdatedAt = reader.GetFieldValue<DateTime>(6),

            PolicyId = reader.IsDBNull(7)
                ? null
                : reader.GetGuid(7),

            PolicyCode = reader.IsDBNull(8)
                ? null
                : reader.GetString(8),

            IsActive = reader.GetBoolean(9)
        };
    }
}
