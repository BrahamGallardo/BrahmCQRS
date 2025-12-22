using BrahmCQRS.Application.DTOs.Auth;

namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Service for managing authentication users.
/// </summary>
public interface IAuthUserService
{
    /// <summary>
    /// Creates a new user account (sends verification email).
    /// </summary>
    Task<AuthUserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms user's email address.
    /// </summary>
    Task<AuthUserDto> ConfirmEmailAsync(string confirmationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets up initial password after email confirmation.
    /// </summary>
    Task<AuthUserDto> SetupPasswordAsync(string setupToken, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes user password (requires current password).
    /// </summary>
    Task<AuthUserDto> ChangePasswordAsync(int userId, ChangePasswordDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a password reset token (sends email).
    /// </summary>
    Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets password using reset token.
    /// </summary>
    Task<AuthUserDto> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user by email (for internal use).
    /// </summary>
    Task<AuthUserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user by ID.
    /// </summary>
    Task<AuthUserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends email confirmation.
    /// </summary>
    Task<bool> ResendConfirmationEmailAsync(int userId, CancellationToken cancellationToken = default);
}
