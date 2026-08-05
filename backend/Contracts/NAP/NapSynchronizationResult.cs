namespace BDIP.Contracts.NAP;

public sealed class NapSynchronizationResult
{
    public int LdapUsers { get; set; }

    public int ExistingUsers { get; set; }

    public int InsertedUsers { get; set; }

    public int FailedUsers { get; set; }
}
