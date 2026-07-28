namespace BDIP.Contracts.Users.Import;

public class ExecuteSynologyUserImportResponse
{
    public int TotalRows { get; set; }

    public int CreatedUsers { get; set; }

    public int SkippedExistingUsers { get; set; }

    public int DisabledUsers { get; set; }

    public int GroupMembershipsAdded { get; set; }

    public List<string> Errors { get; set; } = new();
}
