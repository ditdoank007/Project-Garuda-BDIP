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

        await using var command = dataSource.CreateCommand(
            """
            SELECT
                u.username,
                u.nip,
                u.finger_id,
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

    public async Task<UserResponse?> GetUserByUsernameAsync(string username)
    {
        await using var dataSource = CreateDataSource();

        await using var command = dataSource.CreateCommand(
            """
            SELECT
                u.username,
                u.nip,
                u.finger_id,
                u.full_name,
                u.email,
                un.name AS unit,
                u.enabled
            FROM public.users AS u
            LEFT JOIN public.units AS un
                ON u.unit_id = un.id
            WHERE LOWER(u.username) = LOWER(@username)
            LIMIT 1;
            """);

        command.Parameters.AddWithValue("username", username);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return Map(reader);
    }

    public async Task<int> CountUsersAsync()
    {
        await using var dataSource = CreateDataSource();

        await using var command = dataSource.CreateCommand(
            """
            SELECT COUNT(*)
            FROM public.users;
            """);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        await using var dataSource = CreateDataSource();

        if (await UsernameExistsAsync(dataSource, request.Username))
        {
            throw new InvalidOperationException(
                $"Username '{request.Username}' already exists.");
        }

        var unitId = await FindUnitIdAsync(dataSource, request.Unit);

        await using var command = dataSource.CreateCommand(
            """
            INSERT INTO public.users
            (
                username,
                nip,
                finger_id,
                full_name,
                email,
                unit_id,
                enabled
            )
            VALUES
            (
                @username,
                @nip,
                @fingerid,
                @fullname,
                @email,
                @unitid,
                @enabled
            );
            """);

        command.Parameters.AddWithValue("username", request.Username);
        command.Parameters.AddWithValue(
            "nip",
            string.IsNullOrWhiteSpace(request.Nip) ? DBNull.Value : request.Nip);
        command.Parameters.AddWithValue(
            "fingerid",
            string.IsNullOrWhiteSpace(request.FingerId) ? DBNull.Value : request.FingerId);
        command.Parameters.AddWithValue("fullname", request.FullName);
        command.Parameters.AddWithValue(
            "email",
            string.IsNullOrWhiteSpace(request.Email) ? DBNull.Value : request.Email);
        command.Parameters.AddWithValue(
            "unitid",
            unitId is null ? DBNull.Value : unitId);
        command.Parameters.AddWithValue("enabled", request.Enabled);

        await command.ExecuteNonQueryAsync();

        // BDIP is the administration master.
        await _ldapProvisioning.CreateUserAsync(request);
        await _radiusProvisioning.CreateUserAsync(request);
    }

    public async Task UpdateUserAsync(
        string username,
        UpdateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var newUsername = string.IsNullOrWhiteSpace(request.Username)
            ? username
            : request.Username.Trim();

        if (!string.Equals(
            username,
            newUsername,
            StringComparison.OrdinalIgnoreCase))
        {
            await using var checkDataSource = CreateDataSource();

            if (await UsernameExistsAsync(checkDataSource, newUsername))
            {
                throw new InvalidOperationException(
                    $"Username '{newUsername}' already exists.");
            }
        }

        await using var dataSource = CreateDataSource();

        var unitId = await FindUnitIdAsync(dataSource, request.Unit);

        await using var command = dataSource.CreateCommand(
            """
            UPDATE public.users
            SET
                username  = @newusername,
                nip       = @nip,
                finger_id = @fingerid,
                full_name = @fullname,
                email     = @email,
                unit_id   = @unitid,
                enabled   = @enabled
            WHERE LOWER(username)=LOWER(@username);
            """);

        command.Parameters.AddWithValue("newusername", newUsername);
        command.Parameters.AddWithValue(
            "nip",
            string.IsNullOrWhiteSpace(request.Nip) ? DBNull.Value : request.Nip);
        command.Parameters.AddWithValue(
            "fingerid",
            string.IsNullOrWhiteSpace(request.FingerId) ? DBNull.Value : request.FingerId);
        command.Parameters.AddWithValue("fullname", request.FullName);
        command.Parameters.AddWithValue(
            "email",
            string.IsNullOrWhiteSpace(request.Email) ? DBNull.Value : request.Email);
        command.Parameters.AddWithValue(
            "unitid",
            unitId is null ? DBNull.Value : unitId);
        command.Parameters.AddWithValue("enabled", request.Enabled);
        command.Parameters.AddWithValue("username", username);

        var affected = await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }

        // BDIP database is updated first. Then propagate the same identity
        // change to the downstream identity/authentication stores.
        if (!string.Equals(
            username,
            newUsername,
            StringComparison.OrdinalIgnoreCase))
        {
            await _ldapProvisioning.RenameUserAsync(
                username,
                newUsername);

            await _radiusProvisioning.RenameUserAsync(
                username,
                newUsername);
        }

        await _ldapProvisioning.UpdateUserAsync(
            newUsername,
            request);

        await _ldapProvisioning.UpdateUserStatusAsync(
            newUsername,
            new UpdateUserStatusRequest
            {
                Enabled = request.Enabled
            });
    }

    public async Task ResetPasswordAsync(
        string username,
        ResetUserPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new InvalidOperationException("New password is required.");
        }

        // Password reset originates in BDIP and is immediately propagated.
        // The plaintext password is never stored in BDIP or FreeRADIUS.
        await _ldapProvisioning.ResetPasswordAsync(username, request);
        await _radiusProvisioning.ResetPasswordAsync(username);
    }

    public async Task UpdateUserStatusAsync(
        string username,
        UpdateUserStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var dataSource = CreateDataSource();

        await using var command = dataSource.CreateCommand(
            """
            UPDATE public.users
            SET
                enabled=@enabled,
                updated_at=NOW()
            WHERE LOWER(username)=LOWER(@username);
            """);

        command.Parameters.AddWithValue("enabled", request.Enabled);
        command.Parameters.AddWithValue("username", username);

        var rows = await command.ExecuteNonQueryAsync();

        if (rows == 0)
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }

        await _ldapProvisioning.UpdateUserStatusAsync(username, request);
    }

    public async Task DeleteUserAsync(string username)
    {
        await using var dataSource = CreateDataSource();

        await using var command = dataSource.CreateCommand(
            """
            DELETE FROM public.users
            WHERE LOWER(username)=LOWER(@username);
            """);

        command.Parameters.AddWithValue("username", username);

        var rows = await command.ExecuteNonQueryAsync();

        if (rows == 0)
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }

        await _ldapProvisioning.DeleteUserAsync(username);
        await _radiusProvisioning.RemoveUserGroupAsync(username);
        await _radiusProvisioning.DeleteUserAsync(username);
    }

    private async Task<Guid?> FindUnitIdAsync(
        NpgsqlDataSource dataSource,
        string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
            return null;

        await using var command = dataSource.CreateCommand(
            """
            SELECT id
            FROM public.units
            WHERE LOWER(name)=LOWER(@unit)
            LIMIT 1;
            """);

        command.Parameters.AddWithValue("unit", unitName);

        var result = await command.ExecuteScalarAsync();

        return result is Guid id ? id : null;
    }

    private async Task<bool> UsernameExistsAsync(
        NpgsqlDataSource dataSource,
        string username)
    {
        await using var command = dataSource.CreateCommand(
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
            Nip = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            FingerId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            FullName = reader.GetString(3),
            Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Unit = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            Enabled = reader.GetBoolean(6)
        };
    }
}
