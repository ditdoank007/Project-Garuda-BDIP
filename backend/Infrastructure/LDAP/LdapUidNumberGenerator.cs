using System.DirectoryServices.Protocols;

namespace BDIP.Infrastructure.LDAP;

public static class LdapUidNumberGenerator
{
    public static int GetNext(
        SearchResponse response)
    {
        int max = 10000;

        foreach (SearchResultEntry entry in response.Entries)
        {
            if (entry.Attributes["uidNumber"] == null)
                continue;

            if (int.TryParse(
                entry.Attributes["uidNumber"][0]?.ToString(),
                out int uid))
            {
                if (uid > max)
                    max = uid;
            }
        }

        return max + 1;
    }
}