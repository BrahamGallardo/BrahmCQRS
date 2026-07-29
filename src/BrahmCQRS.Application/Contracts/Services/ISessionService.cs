using BrahmCQRS.Application.DTOs.Auth;

namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Service for managing user authentication sessions.
/// </summary>
/// <remarks>
/// Session timestamps are stored in UTC and aligned with the access token lifetime
/// reported by <see cref="ITokenService.GetAccessTokenLifetime"/>.
/// </remarks>
public interface ISessionService
{
    /// <summary>
    /// Authenticates a user and creates a session.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created session, including the access token and its expiration.</returns>
    Task<SessionDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a new access token for a user that still holds an active, non-expired session.
    /// </summary>
    /// <param name="userId">
    /// The authenticated user identifier. This value MUST be read from the validated
    /// token claims of the incoming request and MUST NEVER be taken from the request body,
    /// query string or route, otherwise any caller could mint tokens for arbitrary users.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refreshed session.</returns>
    /// <exception cref="Domain.Exceptions.InvalidCredentialsException">
    /// Thrown when the user has no active, non-expired session.
    /// </exception>
    Task<SessionDto> RefreshTokenAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by revoking the access token and closing every active session.
    /// </summary>
    /// <param name="token">The raw access token to revoke.</param>
    /// <param name="userId">The authenticated user identifier, taken from the token claims.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogoutAsync(string token, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a token has not been revoked and that the user still holds an
    /// active, non-expired session.
    /// </summary>
    /// <param name="token">The raw access token to validate.</param>
    /// <param name="userId">The authenticated user identifier, taken from the token claims.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the token and the session are both still valid.</returns>
    Task<bool> ValidateTokenAsync(string token, int userId, CancellationToken cancellationToken = default);
}
