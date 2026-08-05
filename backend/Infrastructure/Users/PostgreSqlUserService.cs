using BDIP.Application.Users;
using BDIP.Application.Provisioning;
using BDIP.Contracts.Users;
using BDIP.Contracts.Users.Requests;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Users;

public sealed class PostgreSqlUserService : IUserService
{
private readonly ApplicationDbOptions _options;
private readonly ILdapProvisioningService _ldapProvisioning;
private readonly IRadiusProvisioningService _radiusProvisioning;

    public PostgreSqlUserService(
        IOptions<ApplicationDbOptions> options,
        ILdapProvisioningService ldapProvisioning,
        IRadiusProvisioningService radiusProvisioning)
    {
        _options = options.Value;
        _ldapProvisioning = ldapProvisioning;
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

    public async Task<UserListResponse> GetUsersAsync()
    {
        var result = new UserListResponse();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    u.username,
                    u.full_name,
                    u.email,
                    un.name AS unit,
                    u.enabled
                    FROM public.users AS u
                    LEFT JOIN public.units AS un
                        ON u.unit_id = un.id
                    ORDER BY u.full_name;
                """);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Users.Add(Map(reader));
        }

        return result;
    }

    public async Task<int> CountUsersAsync()
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT COUNT(*)
                FROM public.users;
                """);

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task CreateUserAsync(
        CreateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new InvalidOperationException(
                "Username is required.");
        }

        await using var dataSource = CreateDataSource();

        if (await UsernameExistsAsync(
            dataSource,
            request.Username))
        {
            throw new InvalidOperationException(
                $"Username '{request.Username}' already exists.");
        }

        var unitId =
            await FindUnitIdAsync(
                dataSource,
                request.Unit);

        await using var command =
            dataSource.CreateCommand(
                """
                INSERT INTO public.users
                (
                    username,
                    full_name,
                    email,
                    unit_id,
                    enabled
                )
                VALUES
                (
                    @username,
                    @fullname,
                    @email,
                    @unitid,
                    @enabled
                );
                """);

        command.Parameters.AddWithValue(
            "username",
            request.Username);

        command.Parameters.AddWithValue(
            "fullname",
            request.FullName);

        command.Parameters.AddWithValue(
            "email",
            string.IsNullOrWhiteSpace(request.Email)
                ? DBNull.Value
                : request.Email);

        command.Parameters.AddWithValue(
            "unitid",
            unitId is null
                ? DBNull.Value
                : unitId);

        command.Parameters.AddWithValue(
            "enabled",
            request.Enabled);

        await command.ExecuteNonQueryAsync();

        // Provision otomatis ke OpenLDAP
        await _ldapProvisioning.CreateUserAsync(request);

        // Provision otomatis ke FreeRADIUS
        await _radiusProvisioning.CreateUserAsync(request);
    }

    public async Task UpdateUserAsync(
        string username,
        UpdateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var dataSource = CreateDataSource();

        var unitId =
            await FindUnitIdAsync(
                dataSource,
                request.Unit);

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.users
                SET
                    full_name = @fullname,
                    email     = @email,
                    unit_id   = @unitid,
                    enabled   = @enabled
                WHERE LOWER(username)=LOWER(@username);
                """);

        command.Parameters.AddWithValue(
            "fullname",
            request.FullName);

        command.Parameters.AddWithValue(
            "email",
            string.IsNullOrWhiteSpace(request.Email)
                ? DBNull.Value
                : request.Email);

        command.Parameters.AddWithValue(
            "unitid",
            unitId is null
                ? DBNull.Value
                : unitId);

        command.Parameters.AddWithValue(
            "enabled",
            request.Enabled);

        command.Parameters.AddWithValue(
            "username",
            username);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }
    }

    public Task ResetPasswordAsync(
        string username,
        ResetUserPasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateUserStatusAsync(
        string username,
        UpdateUserStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.users
                SET
                    enabled=@enabled,
                    updated_at=NOW()
                WHERE LOWER(username)=LOWER(@username);
                """);

        command.Parameters.AddWithValue(
            "enabled",
            request.Enabled);

        command.Parameters.AddWithValue(
            "username",
            username);

        var rows =
            await command.ExecuteNonQueryAsync();

        if (rows == 0)
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }
    }

    public async Task DeleteUserAsync(
        string username)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                DELETE FROM public.users
                WHERE LOWER(username)=LOWER(@username);
                """);

        command.Parameters.AddWithValue(
            "username",
            username);

        var rows =
            await command.ExecuteNonQueryAsync();

        if (rows == 0)
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }
            await _radiusProvisioning
                .RemoveUserGroupAsync(username);

            await _radiusProvisioning
                .DeleteUserAsync(username);
    }

    private async Task<Guid?> FindUnitIdAsync(
        NpgsqlDataSource dataSource,
        string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
        {
            return null;
        }

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT id
                FROM public.units
                WHERE LOWER(name)=LOWER(@unit)
                LIMIT 1;
                """);

        command.Parameters.AddWithValue("unit", unitName);

        var result = await command.ExecuteScalarAsync();

        return result is Guid id
            ? id
            : null;
    }

    private async Task<bool> UsernameExistsAsync(
        NpgsqlDataSource dataSource,
        string username)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT COUNT(*)
                FROM public.users
                WHERE LOWER(username)=LOWER(@username);
                """);

        command.Parameters.AddWithValue("username", username);

        var count = (long)(await command.ExecuteScalarAsync() ?? 0);

        return count > 0;
    }

    private static UserResponse Map(NpgsqlDataReader reader)
    {
        return new UserResponse
        {
            Uid = reader.GetString(0),
            Username = reader.GetString(0),
            FullName = reader.GetString(1),
            Email = reader.IsDBNull(2)
            ? string.Empty
            : reader.GetString(2),
            Unit = reader.IsDBNull(3)
            ? string.Empty
            : reader.GetString(3),
            Enabled = reader.GetBoolean(4)
        };
    }
}
