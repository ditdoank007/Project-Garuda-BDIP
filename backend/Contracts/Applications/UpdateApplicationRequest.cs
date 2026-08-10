namespace BDIP.Contracts.Applications;

public class UpdateApplicationRequest
{
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string BaseUrl { get; set; } = "";
}
