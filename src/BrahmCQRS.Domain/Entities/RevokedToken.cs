namespace BrahmCQRS.Domain.Entities;

/// <summary>
/// Represents a revoked JWT token to prevent reuse.
/// </summary>
public class RevokedToken : BaseEntity
{
    /// <summary>
    /// Gets or sets the revoked token string.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the token was revoked.
    /// </summary>
    public DateTime RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who owned this token (optional).
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Gets or sets the reason for revocation (optional).
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets when the revoked token naturally expires, read from the token's
    /// <c>exp</c> claim at revocation time. Null when the expiration could not be determined.
    /// </summary>
    /// <remarks>
    /// Used to purge blacklist entries that are no longer needed because JWT lifetime
    /// validation already rejects the token. Stored in UTC.
    /// </remarks>
    public DateTime? ExpiresAt { get; set; }
}
