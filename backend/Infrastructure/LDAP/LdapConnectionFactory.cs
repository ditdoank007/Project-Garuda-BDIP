using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Net;

namespace BDIP.Infrastructure.LDAP;

public class LdapConnectionFactory : ILdapConnectionFactory
{
    private readonly LdapOptions _options;

    public LdapConnectionFactory(IOptions<LdapOptions> options)
    {
        _options = options.Value;
    }

    public LdapConnection Create()
{
    var identifier = new LdapDirectoryIdentifier(
        _options.Host,
        _options.Port);

    var connection = new LdapConnection(identifier)
    {
        AuthType = AuthType.Basic,
        Credential = new NetworkCredential(
            _options.BindDn,
            _options.Password)
    };

    connection.SessionOptions.ProtocolVersion = 3;
    connection.SessionOptions.SecureSocketLayer = _options.UseSsl;
    connection.Timeout = TimeSpan.FromSeconds(10);

    connection.Bind();

    return connection;
}

}    
