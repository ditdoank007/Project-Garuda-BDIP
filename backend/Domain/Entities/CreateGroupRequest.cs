using System.ComponentModel.DataAnnotations;

namespace BDIP.Contracts.Groups;

public class CreateGroupRequest
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}