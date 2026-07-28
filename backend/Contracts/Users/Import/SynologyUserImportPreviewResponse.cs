namespace BDIP.Contracts.Users.Import;

public class SynologyUserImportPreviewResponse
{
    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int NewUsers { get; set; }

    public int ExistingUsers { get; set; }

    public int DuplicateUsernames { get; set; }

    public int UsersWithoutEmail { get; set; }

    public int DisabledUsers { get; set; }

    public List<SynologyUserImportPreviewItem> Users { get; set; } = new();

    public List<string> GroupsFound { get; set; } = new();
}

public class SynologyUserImportPreviewItem
{
    public int RowNumber { get; set; }

    public string SourceUsername { get; set; } = "";

    public string Username { get; set; } = "";

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Status { get; set; } = "";

    public bool Enabled { get; set; }

    public bool ExistsInLdap { get; set; }

    public bool IsDuplicateInCsv { get; set; }

    public string Action { get; set; } = "";

    public string Note { get; set; } = "";

    public List<string> Groups { get; set; } = new();
}
