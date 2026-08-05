namespace BDIP.Contracts.Units;

public class UnitResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public Guid LocationId { get; set; }

    public string LocationName { get; set; } = "";

    public bool IsActive { get; set; }
}