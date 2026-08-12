using BDIP.Application.ApplicationAccess;
using BDIP.Contracts.ApplicationAccess;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.ApplicationAccess;

public sealed class PostgreSqlApplicationAccessService
    : IApplicationAccessService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlApplicationAccessService(
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
            ApplicationName = "BDIP Backend"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<List<ApplicationAccessResponse>> GetAllAsync()
    {
        var result = new List<ApplicationAccessResponse>();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    aa.id,
                    aa.user_id,
                    u.username,
                    u.full_name,
                    aa.application_id,
                    a.code,
                    a.name,
                    aa.is_active,
                    aa.created_at,
                    aa.updated_at
                FROM public.application_access aa
                INNER JOIN public.users u
                    ON u.id = aa.user_id
                INNER JOIN public.applications a
                    ON a.id = aa.application_id
                ORDER BY
                    u.full_name,
                    a.name;
                """);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public async Task<ApplicationAccessResponse?> GetByIdAsync(
        Guid id)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    aa.id,
                    aa.user_id,
                    u.username,
                    u.full_name,
                    aa.application_id,
                    a.code,
                    a.name,
                    aa.is_active,
                    aa.created_at,
                    aa.updated_at
                FROM public.application_access aa
                INNER JOIN public.users u
                    ON u.id = aa.user_id
                INNER JOIN public.applications a
                    ON a.id = aa.application_id
                WHERE aa.id = @id;
                """);

        command.Parameters.AddWithValue("id", id);

        await using var reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? Map(reader)
            : null;
    }

    public async Task<ApplicationAccessResponse> CreateAsync(
        CreateApplicationAccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "User is required.");
        }

        if (request.ApplicationId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Application is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var userCommand =
            dataSource.CreateCommand(
                """
                SELECT 1
                FROM public.users
                WHERE id = @user_id;
                """);

        userCommand.Parameters.AddWithValue(
            "user_id",
            request.UserId);

        var userExists = await userCommand.ExecuteScalarAsync();

        if (userExists is null)
        {
            throw new InvalidOperationException(
                "User was not found.");
        }

        await using var applicationCommand =
            dataSource.CreateCommand(
                """
                SELECT 1
                FROM public.applications
                WHERE id = @application_id;
                """);

        applicationCommand.Parameters.AddWithValue(
            "application_id",
            request.ApplicationId);

        var applicationExists =
            await applicationCommand.ExecuteScalarAsync();

        if (applicationExists is null)
        {
            throw new InvalidOperationException(
                "Application was not found.");
        }

        await using var duplicateCommand =
            dataSource.CreateCommand(
                """
                SELECT 1
                FROM public.application_access
                WHERE user_id = @user_id
                  AND application_id = @application_id;
                """);

        duplicateCommand.Parameters.AddWithValue(
            "user_id",
            request.UserId);

        duplicateCommand.Parameters.AddWithValue(
            "application_id",
            request.ApplicationId);

        if (await duplicateCommand.ExecuteScalarAsync()
            is not null)
        {
            throw new InvalidOperationException(
                "Application access already exists for this user.");
        }

        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var insertCommand =
            dataSource.CreateCommand(
                """
                INSERT INTO public.application_access
                (
                    id,
                    user_id,
                    application_id,
                    is_active,
                    created_at,
                    updated_at
                )
                VALUES
                (
                    @id,
                    @user_id,
                    @application_id,
                    @is_active,
                    @created_at,
                    @updated_at
                );
                """);

        insertCommand.Parameters.AddWithValue("id", id);
        insertCommand.Parameters.AddWithValue(
            "user_id",
            request.UserId);
        insertCommand.Parameters.AddWithValue(
            "application_id",
            request.ApplicationId);
        insertCommand.Parameters.AddWithValue(
            "is_active",
            true);
        insertCommand.Parameters.AddWithValue(
            "created_at",
            now);
        insertCommand.Parameters.AddWithValue(
            "updated_at",
            now);

        await insertCommand.ExecuteNonQueryAsync();

        var created = await GetByIdAsync(id);

        if (created is null)
        {
            throw new InvalidOperationException(
                "Application access was not found after create.");
        }

        return created;
    }

    public async Task<ApplicationAccessResponse> UpdateAsync(
        Guid id,
        UpdateApplicationAccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (id == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Application access id is required.");
        }

        await using var dataSource = CreateDataSource();

        var now = DateTime.UtcNow;

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.application_access
                SET
                    is_active = @is_active,
                    updated_at = @updated_at
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue(
            "is_active",
            request.IsActive);

        command.Parameters.AddWithValue(
            "updated_at",
            now);

        command.Parameters.AddWithValue(
            "id",
            id);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                "Application access was not found.");
        }

        var updated = await GetByIdAsync(id);

        if (updated is null)
        {
            throw new InvalidOperationException(
                "Application access was not found after update.");
        }

        return updated;
    }

    public async Task DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Application access id is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                DELETE FROM public.application_access
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", id);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                "Application access was not found.");
        }
    }

    private static ApplicationAccessResponse Map(
        NpgsqlDataReader reader)
    {
        return new ApplicationAccessResponse
        {
            Id = reader.GetGuid(0),
            UserId = reader.GetGuid(1),
            Username = reader.GetString(2),
            FullName = reader.GetString(3),
            ApplicationId = reader.GetGuid(4),
            ApplicationCode = reader.GetString(5),
            ApplicationName = reader.GetString(6),
            IsActive = reader.GetBoolean(7),
            CreatedAt = reader.GetDateTime(8),
            UpdatedAt = reader.GetDateTime(9)
        };
    }
}
