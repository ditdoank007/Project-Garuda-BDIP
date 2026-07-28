using System.DirectoryServices.Protocols;

namespace BDIP.Infrastructure.LDAP;

public interface ILdapConnectionFactory
{
    LdapConnection Create();
}