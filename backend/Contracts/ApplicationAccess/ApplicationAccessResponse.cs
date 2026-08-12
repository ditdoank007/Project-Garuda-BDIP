namespace BDIP.Contracts.ApplicationAccess;

public class ApplicationAccessResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = "";

    public string FullName { get; set; } = "";

    public Guid ApplicationId { get; set; }

    public string ApplicationCode { get; set; } = "";

    public string ApplicationName { get; set; } = "";

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
