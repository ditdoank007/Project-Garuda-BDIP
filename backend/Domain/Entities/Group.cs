namespace BDIP.Domain.Entities;

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Common Name (cn)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// LDAP Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// POSIX Group ID
    /// </summary>
    public int GidNumber { get; set; }

    /// <summary>
    /// Distinguished Name
    /// </summary>
    public string DistinguishedName { get; set; } = string.Empty;

    /// <summary>
    /// LDAP Members (DN)
    /// </summary>
    public List<string> Members { get; set; } = new();

    /// <summary>
    /// Object Classes
    /// </summary>
    public List<string> ObjectClasses { get; set; } = new()
    {
        "top",
        "groupOfNames",
        "posixGroup"
    };

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}