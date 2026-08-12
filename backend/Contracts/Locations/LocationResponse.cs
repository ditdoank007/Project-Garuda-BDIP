namespace BDIP.Contracts.Locations;

public class LocationResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string Type { get; set; } = "";

    public int UnitCount { get; set; }
}
