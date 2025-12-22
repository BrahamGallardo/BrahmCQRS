namespace BrahmCQRS.Domain.Entities;

/// <summary>
/// Represents a user role for authorization.
/// </summary>
public class AuthRole : BaseEntity
{
    /// <summary>
    /// Gets or sets the role name (e.g., Admin, User, Manager).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to users with this role.
    /// </summary>
    public virtual ICollection<AuthUser>? Users { get; set; }
}
