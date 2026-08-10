namespace BDIP.Contracts.Applications;

public class CreateApplicationRequest
{
    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string BaseUrl { get; set; } = "";
}
