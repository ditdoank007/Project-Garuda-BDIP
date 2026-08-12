namespace BDIP.Contracts.ApplicationAccess;

public class CreateApplicationAccessRequest
{
    public Guid UserId { get; set; }

    public Guid ApplicationId { get; set; }
}
