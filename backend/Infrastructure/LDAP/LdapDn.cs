namespace BDIP.Infrastructure.LDAP;

public static class LdapDn
{
    // Base DN
    public const string BaseDn = "dc=bdip,dc=local";

    // Organizational Units
    public const string UsersOu = $"ou=Users,{BaseDn}";

    public const string GroupsOu = $"ou=Groups,{BaseDn}";

    public const string RolesOu = $"ou=Roles,{BaseDn}";

    public const string ApplicationsOu = $"ou=Applications,{BaseDn}";

    public const string ServiceAccountsOu = $"ou=ServiceAccounts,{BaseDn}";

    public const string LocationsOu = $"ou=Locations,{BaseDn}";

    public const string SystemOu = $"ou=System,{BaseDn}";

    // Placeholder member
    public const string PlaceholderMember =
        $"cn=LDAP Placeholder,{SystemOu}";

    public static string UserDn(string uid)
        => $"uid={uid},{UsersOu}";

    public static string GroupDn(string cn)
        => $"cn={cn},{GroupsOu}";

    public static string RoleDn(string cn)
        => $"cn={cn},{RolesOu}";
}