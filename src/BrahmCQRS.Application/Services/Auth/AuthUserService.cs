using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Auth;
using BrahmCQRS.Application.DTOs.Email;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Domain.Exceptions;
using BrahmCQRS.Domain.Specifications.Auth;
using BrahmCQRS.Shared.Security;

namespace BrahmCQRS.Application.Services.Auth;

/// <summary>
/// Implementation of authentication user service.
/// </summary>
/// <remarks>
/// Password changes and resets close every active session of the user: already issued
/// access tokens cannot be revoked individually because they are not stored, but
/// closing the sessions invalidates them through session validation.
/// </remarks>
public class AuthUserService : IAuthUserService
{
    private readonly ICommandRepository<AuthUser> _userCommandRepo;
    private readonly IQueryRepository<AuthUser> _userQueryRepo;
    private readonly ICommandRepository<AuthSession> _sessionCommandRepo;
    private readonly IQueryRepository<AuthSession> _sessionQueryRepo;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthUserService(
        ICommandRepository<AuthUser> userCommandRepo,
        IQueryRepository<AuthUser> userQueryRepo,
        ICommandRepository<AuthSession> sessionCommandRepo,
        IQueryRepository<AuthSession> sessionQueryRepo,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userCommandRepo = userCommandRepo;
        _userQueryRepo = userQueryRepo;
        _sessionCommandRepo = sessionCommandRepo;
        _sessionQueryRepo = sessionQueryRepo;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<AuthUserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // Deactivated users must count as duplicates: the Email column is unique,
        // so skipping them would surface a raw DbUpdateException instead.
        var existingUser = await GetByEmailInternalAsync(request.Email, includeDisabled: true, cancellationToken);
        if (existingUser != null)
            throw new DuplicateEntityException(nameof(AuthUser), request.Email);

        // Create user
        var user = new AuthUser
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            RoleId = request.RoleId,
            PasswordHash = string.Empty,
            EmailVerified = false,
            HasPassword = false
            // Activated / CreatedDate are set automatically by BaseDbContext.
        };

        var createdUser = await _userCommandRepo.CreateAsync(user, cancellationToken);

        // Generate confirmation token
        var confirmationToken = _tokenService.GenerateConfirmationToken(createdUser.Id);

        // Send confirmation email
        await _emailService.SendEmailAsync(new EmailDto
        {
            To = createdUser.Email,
            Subject = "Confirm your email",
            Body = $"Click here to confirm: {confirmationToken}"
        }, cancellationToken);

        return MapToDto(createdUser);
    }

    public async Task<AuthUserDto> ConfirmEmailAsync(string confirmationToken, CancellationToken cancellationToken = default)
    {
        var userId = await _tokenService.ValidateAndExtractUserIdAsync(confirmationToken, cancellationToken);
        if (userId == null)
            throw new InvalidCredentialsException("Invalid or expired confirmation token");

        var user = await _userQueryRepo.GetByIdAsync(userId.Value, false, cancellationToken);
        if (user == null)
            throw new EntityNotFoundException(nameof(AuthUser), userId.Value);

        user.EmailVerified = true;
        await _userCommandRepo.UpdateAsync(user, cancellationToken);

        // Generate setup token
        var setupToken = _tokenService.GenerateSetupToken(user.Id);

        // Send setup password email
        await _emailService.SendEmailAsync(new EmailDto
        {
            To = user.Email,
            Subject = "Setup your password",
            Body = $"Setup token: {setupToken}"
        }, cancellationToken);

        return MapToDto(user);
    }

    public async Task<AuthUserDto> SetupPasswordAsync(string setupToken, string newPassword, CancellationToken cancellationToken = default)
    {
        var userId = await _tokenService.ValidateAndExtractUserIdAsync(setupToken, cancellationToken);
        if (userId == null)
            throw new InvalidCredentialsException("Invalid or expired setup token");

        var user = await _userQueryRepo.GetByIdAsync(userId.Value, false, cancellationToken);
        if (user == null)
            throw new EntityNotFoundException(nameof(AuthUser), userId.Value);

        // Hash password
        user.PasswordHash = newPassword.EncryptPassword();
        user.HasPassword = true;

        await _userCommandRepo.UpdateAsync(user, cancellationToken);
        await _tokenService.RevokeTokenAsync(setupToken, userId, "Password setup completed", cancellationToken);

        return MapToDto(user);
    }

    public async Task<AuthUserDto> ChangePasswordAsync(int userId, ChangePasswordDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userQueryRepo.GetByIdAsync(userId, false, cancellationToken);
        if (user == null)
            throw new EntityNotFoundException(nameof(AuthUser), userId);

        // Verify current password
        if (!user.PasswordHash.ValidatePassword(request.CurrentPassword))
            throw new InvalidCredentialsException("Current password is incorrect");

        // Check that new password is different
        if (user.PasswordHash.ValidatePassword(request.NewPassword))
            throw new InvalidOperationException("New password must be different from current password");

        // Hash new password
        user.PasswordHash = request.NewPassword.EncryptPassword();
        await _userCommandRepo.UpdateAsync(user, cancellationToken);

        // Already-issued access tokens cannot be revoked individually because they are
        // not stored, but closing the sessions invalidates them through session validation.
        await CloseActiveSessionsAsync(userId, cancellationToken);

        return MapToDto(user);
    }

    public async Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetByEmailInternalAsync(email, includeDisabled: false, cancellationToken);
        if (user == null)
            return false; // Don't reveal if email exists

        var resetToken = _tokenService.GeneratePasswordResetToken(user.Id);

        await _emailService.SendEmailAsync(new EmailDto
        {
            To = user.Email,
            Subject = "Password Reset Request",
            Body = $"Reset token: {resetToken}"
        }, cancellationToken);

        return true;
    }

    public async Task<AuthUserDto> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
    {
        var userId = await _tokenService.ValidateAndExtractUserIdAsync(request.Token, cancellationToken);
        if (userId == null)
            throw new InvalidCredentialsException("Invalid or expired reset token");

        var user = await _userQueryRepo.GetByIdAsync(userId.Value, false, cancellationToken);
        if (user == null)
            throw new EntityNotFoundException(nameof(AuthUser), userId.Value);

        user.PasswordHash = request.NewPassword.EncryptPassword();
        await _userCommandRepo.UpdateAsync(user, cancellationToken);
        await _tokenService.RevokeTokenAsync(request.Token, userId, "Password reset completed", cancellationToken);

        // Any session opened before the reset must not survive it.
        await CloseActiveSessionsAsync(userId.Value, cancellationToken);

        return MapToDto(user);
    }

    public async Task<AuthUserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetByEmailInternalAsync(email, includeDisabled: false, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<AuthUserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Uses a specification instead of GetByIdAsync so that RoleName is populated.
        var user = await _userQueryRepo.FirstOrDefaultAsync(new GetUserByIdSpec(userId), cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<bool> ResendConfirmationEmailAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userQueryRepo.GetByIdAsync(userId, false, cancellationToken);
        if (user == null || user.EmailVerified)
            return false;

        var confirmationToken = _tokenService.GenerateConfirmationToken(user.Id);

        await _emailService.SendEmailAsync(new EmailDto
        {
            To = user.Email,
            Subject = "Confirm your email",
            Body = $"Click here to confirm: {confirmationToken}"
        }, cancellationToken);

        return true;
    }

    /// <summary>
    /// Gets a user entity by email using an indexed database query.
    /// </summary>
    /// <param name="email">The email address to match.</param>
    /// <param name="includeDisabled">Whether deactivated users should also be returned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching user, or null when none exists.</returns>
    private async Task<AuthUser?> GetByEmailInternalAsync(string email, bool includeDisabled, CancellationToken cancellationToken)
    {
        return await _userQueryRepo.FirstOrDefaultAsync(
            new GetUserByEmailSpec(email, includeDisabled),
            cancellationToken);
    }

    /// <summary>
    /// Deactivates every active session of a user.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of sessions closed.</returns>
    private async Task<int> CloseActiveSessionsAsync(int userId, CancellationToken cancellationToken)
    {
        var sessions = await _sessionQueryRepo.ListAsync(
            new GetActiveSessionsByUserSpec(userId),
            cancellationToken);

        if (sessions.Count == 0)
            return 0;

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        // Only IsActive is flipped: the row is kept queryable for auditing, so
        // Activated (soft delete) is intentionally left untouched.
        await _sessionCommandRepo.UpdateRangeAsync(sessions, cancellationToken);

        return sessions.Count;
    }

    private static AuthUserDto MapToDto(AuthUser user)
    {
        return new AuthUserDto
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            EmailVerified = user.EmailVerified,
            HasPassword = user.HasPassword,
            Activated = user.Activated,
            CreatedDate = user.CreatedDate
        };
    }
}
