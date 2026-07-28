using BDIP.Application.Common;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;

namespace BDIP.Infrastructure.LDAP;

public class LdapNumberGenerator : ILdapNumberGenerator
{
    private readonly ILdapConnectionFactory _connectionFactory;
    private readonly LdapOptions _options;

    public LdapNumberGenerator(
        ILdapConnectionFactory connectionFactory,
        IOptions<LdapOptions> options)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
    }

    public async Task<int> GenerateUidNumberAsync()
    {
        return await Task.Run(() =>
        {
            using var connection = _connectionFactory.Create();

            var request = new SearchRequest(
                _options.PeopleDn,
                "(uidNumber=*)",
                SearchScope.OneLevel,
                "uidNumber");

            var response =
                (SearchResponse)connection.SendRequest(request);

            var max = 10000;

            foreach (SearchResultEntry entry in response.Entries)
            {
                if (entry.Attributes["uidNumber"] == null)
                    continue;

                if (int.TryParse(
                        entry.Attributes["uidNumber"][0]?.ToString(),
                        out var uid))
                {
                    if (uid > max)
                        max = uid;
                }
            }

            return max + 1;
        });
    }

    public async Task<int> GenerateGidNumberAsync()
    {
        return await Task.Run(() =>
        {
            using var connection = _connectionFactory.Create();

            var request = new SearchRequest(
                _options.GroupsDn,
                "(gidNumber=*)",
                SearchScope.OneLevel,
                "gidNumber");

            var response =
                (SearchResponse)connection.SendRequest(request);

            var max = 10000;

            foreach (SearchResultEntry entry in response.Entries)
            {
                if (entry.Attributes["gidNumber"] == null)
                    continue;

                if (int.TryParse(
                        entry.Attributes["gidNumber"][0]?.ToString(),
                        out var gid))
                {
                    if (gid > max)
                        max = gid;
                }
            }

            return max + 1;
        });
    }
}