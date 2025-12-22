namespace BrahmCQRS.Domain.Entities;

/// <summary>
/// Represents an active user session.
/// </summary>
public class AuthSession : BaseEntity
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets whether the session is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when the session expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public virtual AuthUser? User { get; set; }
}
