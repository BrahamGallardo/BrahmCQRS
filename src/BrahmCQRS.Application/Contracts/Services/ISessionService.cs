using BrahmCQRS.Application.DTOs.Auth;

namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Service for managing user authentication sessions.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Authenticates a user and creates a session.
    /// </summary>
    Task<SessionDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an existing session token.
    /// </summary>
    Task<SessionDto> RefreshTokenAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by revoking their token.
    /// </summary>
    Task LogoutAsync(string token, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a token is still valid and not revoked.
    /// </summary>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
