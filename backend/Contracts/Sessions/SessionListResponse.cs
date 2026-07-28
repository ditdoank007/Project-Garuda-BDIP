namespace BDIP.Contracts.Sessions;

public class SessionListResponse
{
    public int Total { get; set; }

    public List<SessionResponse> Sessions { get; set; } = new();
}
