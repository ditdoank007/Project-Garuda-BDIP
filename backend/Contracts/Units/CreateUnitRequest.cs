namespace BDIP.Contracts.Units;

public class CreateUnitRequest
{

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public Guid LocationId { get; set; }
}