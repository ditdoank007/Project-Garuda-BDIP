using BDIP.Application.Provisioning;
using BDIP.Contracts.Users.Requests;
using BDIP.Persistence.PostgreSQL;
using BDIP.Domain.NAP;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Provisioning;

public sealed class PostgreSqlRadiusProvisioningService
    : IRadiusProvisioningService
{
    private readonly RadiusDbOptions _options;

    public PostgreSqlRadiusProvisioningService(
        IOptions<RadiusDbOptions> options)
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
            SslMode = SslMode.Disable
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
            """
            INSERT INTO public.radcheck
            (
                username,
                attribute,
                op,
                value
            )
            VALUES
            (
                @username,
                'Cleartext-Password',
                ':=',
                @password
            );
            """);

        command.Parameters.AddWithValue(
            "username",
            request.Username);

        command.Parameters.AddWithValue(
            "password",
            request.Password);

        await command.ExecuteNonQueryAsync();
    }
    public async Task SyncPolicyAsync(
        Policy policy)
    {
        await using var dataSource =
            CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
            """
            DELETE
            FROM public.radgroupreply
            WHERE groupname=@groupname;
            """);

        command.Parameters.AddWithValue(
            "groupname",
            policy.Code);

        await command.ExecuteNonQueryAsync();
        await using var insert =
        dataSource.CreateCommand(
        """
        INSERT INTO public.radgroupreply
        (
            groupname,
            attribute,
            op,
            value
        )
        VALUES
        (
            @groupname,
            'Session-Timeout',
            '=',
            @value
        );
        """);

    insert.Parameters.AddWithValue(
        "groupname",
        policy.Code);

    insert.Parameters.AddWithValue(
        "value",
        policy.SessionTimeout.ToString());

    await insert.ExecuteNonQueryAsync();
    if (policy.IdleTimeout > 0)
        {
            await using var idle =
                dataSource.CreateCommand(
                """
                INSERT INTO public.radgroupreply
                (
                    groupname,
                    attribute,
                    op,
                    value
                )
                VALUES
                (
                    @groupname,
                    'Idle-Timeout',
                    '=',
                    @value
                );
                """);

            idle.Parameters.AddWithValue(
                "groupname",
                policy.Code);

            idle.Parameters.AddWithValue(
                "value",
                policy.IdleTimeout.ToString());

            await idle.ExecuteNonQueryAsync();
        }
        if (policy.DownloadRate > 0 ||
            policy.UploadRate > 0)
        {
            await using var rate =
                dataSource.CreateCommand(
                """
                INSERT INTO public.radgroupreply
                (
                    groupname,
                    attribute,
                    op,
                    value
                )
                VALUES
                (
                    @groupname,
                    'Mikrotik-Rate-Limit',
                    '=',
                    @value
                );
                """);

            rate.Parameters.AddWithValue(
                "groupname",
                policy.Code);

            rate.Parameters.AddWithValue(
                "value",
                $"{policy.DownloadRate}k/{policy.UploadRate}k");

            await rate.ExecuteNonQueryAsync();
        }
        if (!string.IsNullOrWhiteSpace(policy.IpPool))
        {
            await using var pool =
                dataSource.CreateCommand(
                """
                INSERT INTO public.radgroupreply
                (
                    groupname,
                    attribute,
                    op,
                    value
                )
                VALUES
                (
                    @groupname,
                    'Framed-Pool',
                    '=',
                    @value
                );
                """);

            pool.Parameters.AddWithValue(
                "groupname",
                policy.Code);

            pool.Parameters.AddWithValue(
                "value",
                policy.IpPool);

            await pool.ExecuteNonQueryAsync();
        }
        if (!string.IsNullOrWhiteSpace(policy.AddressList))
        {
            await using var address =
                dataSource.CreateCommand(
                """
                INSERT INTO public.radgroupreply
                (
                    groupname,
                    attribute,
                    op,
                    value
                )
                VALUES
                (
                    @groupname,
                    'Mikrotik-Address-List',
                    '=',
                    @value
                );
                """);

            address.Parameters.AddWithValue(
                "groupname",
                policy.Code);

            address.Parameters.AddWithValue(
                "value",
                policy.AddressList);

            await address.ExecuteNonQueryAsync();
        }
    }

    public async Task DeletePolicyAsync(
        string policyCode)
    {
        await using var dataSource =
            CreateDataSource();

        await using var reply =
            dataSource.CreateCommand(
            """
            DELETE
            FROM public.radgroupreply
            WHERE groupname=@groupname;
            """);

        reply.Parameters.AddWithValue(
            "groupname",
            policyCode);

        await reply.ExecuteNonQueryAsync();
    }

    public async Task AssignUserGroupAsync(
        string username,
        string policyCode)
    {
        await using var dataSource =
            CreateDataSource();

        // Hapus mapping lama
        await using (var delete =
            dataSource.CreateCommand(
            """
            DELETE
            FROM public.radusergroup
            WHERE username=@username;
            """))
        {
            delete.Parameters.AddWithValue(
                "username",
                username);

            await delete.ExecuteNonQueryAsync();
        }

        // Tambahkan mapping baru
        await using (var insert =
            dataSource.CreateCommand(
            """
            INSERT INTO public.radusergroup
            (
                username,
                groupname,
                priority
            )
            VALUES
            (
                @username,
                @groupname,
                1
            );
            """))
        {
            insert.Parameters.AddWithValue(
                "username",
                username);

            insert.Parameters.AddWithValue(
                "groupname",
                policyCode);

            await insert.ExecuteNonQueryAsync();
        }
    }

    public async Task RemoveUserGroupAsync(
        string username)
    {
        await using var dataSource =
            CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
            """
            DELETE
            FROM public.radusergroup
            WHERE username=@username;
            """);

        command.Parameters.AddWithValue(
            "username",
            username);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteUserAsync(
        string username)
    {
        await using var dataSource =
            CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
            """
            DELETE
            FROM public.radcheck
            WHERE username=@username;
            """);

        command.Parameters.AddWithValue(
            "username",
            username);

        await command.ExecuteNonQueryAsync();
    }
}