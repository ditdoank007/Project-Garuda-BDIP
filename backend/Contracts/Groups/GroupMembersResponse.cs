namespace BDIP.Contracts.Groups;

public class GroupMembersResponse
{
    public string GroupName { get; set; } = "";

    public List<GroupMemberResponse> Members { get; set; } = new();

    public int Total => Members.Count;
}
