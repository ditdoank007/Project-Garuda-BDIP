using System.DirectoryServices.Protocols;

using BDIP.Infrastructure.LDAP;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Users;

public sealed class LdapUserImporter
{
    private readonly ILdapConnectionFactory _ldap;
    private readonly LdapOptions _options;
    private readonly ApplicationDbOptions _dbOptions;

    public LdapUserImporter(
        ILdapConnectionFactory ldap,
        IOptions<LdapOptions> options,
        IOptions<ApplicationDbOptions> dbOptions)
    {
        _ldap = ldap;
        _options = options.Value;
        _dbOptions = dbOptions.Value;
    }

    public async Task<int> ImportAsync()
    {
        var imported = 0;

        await using var dataSource = CreateDataSource();

        using var connection = _ldap.Create();

        var request = new SearchRequest(
            _options.PeopleDn,
            "(objectClass=inetOrgPerson)",
            SearchScope.Subtree,
            "uid",
            "cn",
            "mail",
            "ou",
            "shadowExpire",
            "uidNumber");

        var response =
            (SearchResponse)connection.SendRequest(request);

        foreach (SearchResultEntry entry in response.Entries)
        {
            var username = "";

            try
            {
                username =
                    entry.Attributes["uid"]?[0]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(username))
                {
                    continue;
                }

                Guid? unitId = null;

                var unitName =
                    entry.Attributes["ou"]?[0]?.ToString();

                if (!string.IsNullOrWhiteSpace(unitName))
                {
                    unitId = await FindUnitIdAsync(dataSource, unitName);
                }

                var existingCount = await CountUserAsync(dataSource, username);

                if (existingCount > 0)
                {
                    continue;
                }

                var fullname =
                    entry.Attributes["cn"]?[0]?.ToString() ?? "";
                var email =
                    entry.Attributes["mail"]?[0]?.ToString() ?? "";
                var shadowExpire =
                    entry.Attributes["shadowExpire"]?[0]?.ToString();
                int? uidNumber = null;

                if (int.TryParse(
                    entry.Attributes["uidNumber"]?[0]?.ToString(),
                    out var parsedUidNumber))
                {
                    uidNumber = parsedUidNumber;
                }

                var enabled = string.IsNullOrWhiteSpace(shadowExpire) ||
                    shadowExpire == "-1";

                await InsertUserAsync(
                    dataSource,
                    username,
                    fullname,
                    email,
                    unitId,
                    enabled,
                    entry.DistinguishedName,
                    uidNumber);

                imported++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed importing '{username}': {ex.Message}");

                continue;
            }
        }

        return imported;
    }

    private NpgsqlDataSource CreateDataSource()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _dbOptions.Host,
            Port = _dbOptions.Port,
            Database = _dbOptions.Database,
            Username = _dbOptions.Username,
            Password = _dbOptions.Password,
            SslMode = SslMode.Disable,
            Timeout = 10,
            CommandTimeout = 15,
            ApplicationName = "BDIP Backend"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    private async Task<Guid?> FindUnitIdAsync(
        NpgsqlDataSource dataSource,
        string unitName)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT id
                FROM public.units
                WHERE LOWER(name)=LOWER(@unit);
                """);

        command.Parameters.AddWithValue("unit", unitName);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return reader.GetGuid(0);
        }

        return null;
    }

    private async Task<int> CountUserAsync(
        NpgsqlDataSource dataSource,
        string username)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT COUNT(*)
                FROM public.users
                WHERE username=@username;
                """);

        command.Parameters.AddWithValue("username", username);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return reader.GetInt32(0);
        }

        return 0;
    }

    private async Task InsertUserAsync(
        NpgsqlDataSource dataSource,
        string username,
        string fullname,
        string email,
        Guid? unitId,
        bool enabled,
        string ldapDn,
        int? uidNumber)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                INSERT INTO public.users
                (
                    username,
                    full_name,
                    email,
                    unit_id,
                    enabled,
                    ldap_dn,
                    ldap_uid_number
                )
                VALUES
                (
                    @username,
                    @fullname,
                    @email,
                    @unitid,
                    @enabled,
                    @ldapdn,
                    @uidnumber
                );
                """);

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("fullname", fullname);
        command.Parameters.AddWithValue(
            "email",
            string.IsNullOrWhiteSpace(email)
                ? (object)DBNull.Value
                : email);
        command.Parameters.AddWithValue(
            "unitid",
            unitId.HasValue
                ? unitId.Value
                : (object)DBNull.Value);
        command.Parameters.AddWithValue("enabled", enabled);
        command.Parameters.AddWithValue("ldapdn", ldapDn);
        command.Parameters.AddWithValue(
            "uidnumber",
            uidNumber.HasValue
                ? uidNumber.Value
                : (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }
}
