using BDIP.Domain.Common;

namespace BDIP.Domain.Entities;

public class User : BaseEntity
{
    public string Uid { get; set; } = string.Empty;

    public string Cn { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
}