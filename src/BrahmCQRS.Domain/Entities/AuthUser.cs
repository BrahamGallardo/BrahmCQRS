namespace BrahmCQRS.Domain.Entities;

/// <summary>
/// Base entity for user authentication and authorization.
/// Projects can extend this entity to add custom properties (e.g., ClientId, Department, etc.).
/// </summary>
public class AuthUser : BaseEntity
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's email address (unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BCrypt hashed password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role identifier.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets whether the user's email has been verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets whether the user has set up their password.
    /// </summary>
    public bool HasPassword { get; set; }

    /// <summary>
    /// Navigation property to the user's role.
    /// </summary>
    public virtual AuthRole? Role { get; set; }
}
