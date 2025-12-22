namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Service contract for accessing current authenticated user information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's identifier.
    /// </summary>
    /// <returns>The user identifier, or null if no user is authenticated.</returns>
    string? GetCurrentUserId();

    /// <summary>
    /// Gets the current user's email.
    /// </summary>
    /// <returns>The user email, or null if no user is authenticated.</returns>
    string? GetCurrentUserEmail();

    /// <summary>
    /// Gets whether a user is currently authenticated.
    /// </summary>
    /// <returns>True if a user is authenticated; otherwise, false.</returns>
    bool IsAuthenticated();
}
