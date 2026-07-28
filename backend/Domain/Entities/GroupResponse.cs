namespace BDIP.Contracts.Groups;

public class GroupResponse
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int GidNumber { get; set; }

    public string DistinguishedName { get; set; } = string.Empty;

    public int MemberCount { get; set; }

    public DateTime CreatedAt { get; set; }
}