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
}
