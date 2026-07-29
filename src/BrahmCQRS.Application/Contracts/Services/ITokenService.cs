using BrahmCQRS.Application.DTOs.Auth;

namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    string GenerateAccessToken(AuthUserDto user);

    /// <summary>
    /// Generates a confirmation token for email verification.
    /// </summary>
    string GenerateConfirmationToken(int userId);

    /// <summary>
    /// Generates a token for password reset.
    /// </summary>
    string GeneratePasswordResetToken(int userId);

    /// <summary>
    /// Generates a token for initial password setup.
    /// </summary>
    string GenerateSetupToken(int userId);

    /// <summary>
    /// Validates a token and extracts user ID.
    /// </summary>
    Task<int?> ValidateAndExtractUserIdAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a token (adds to blacklist).
    /// </summary>
    Task RevokeTokenAsync(string token, int? userId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a token has been revoked.
    /// </summary>
    Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the lifetime applied to access tokens issued for the specified role.
    /// </summary>
    /// <param name="roleName">The role name, or null to use the default lifetime.</param>
    /// <returns>
    /// The configured lifetime, honoring per-role overrides when the role has one.
    /// </returns>
    /// <remarks>
    /// Exposed so that the Application layer can align session expiration with the real
    /// token lifetime without depending on Infrastructure JWT configuration.
    /// </remarks>
    TimeSpan GetAccessTokenLifetime(string? roleName);

    /// <summary>
    /// Soft deletes blacklist entries whose underlying tokens have already expired.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of purged records.</returns>
    /// <remarks>
    /// Called opportunistically on every revocation, and safe to call from a scheduled
    /// maintenance job. Purged rows are kept for auditing but no longer participate in
    /// revocation checks; delete them physically from the database if disk usage matters.
    /// </remarks>
    Task<int> PurgeExpiredRevokedTokensAsync(CancellationToken cancellationToken = default);
}
