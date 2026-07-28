namespace BDIP.Infrastructure.LDAP;

public class LdapOptions
{
    public string Host { get; set; } = "";

    public int Port { get; set; }

    public bool UseSsl { get; set; }

    public string BaseDn { get; set; } = "";

    public string PeopleDn { get; set; } = "";

    public string GroupsDn { get; set; } = "";

    public string BindDn { get; set; } = "";

    public string Password { get; set; } = "";

    public string PlaceholderMemberDn { get; set; } = "";
}