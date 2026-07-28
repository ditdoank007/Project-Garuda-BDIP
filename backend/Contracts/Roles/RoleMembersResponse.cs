namespace BDIP.Contracts.Roles;

public class RoleMembersResponse
{
    public string RoleName { get; set; } = "";

    public List<RoleMemberResponse> Members { get; set; } = new();

    public int Total => Members.Count;
}
