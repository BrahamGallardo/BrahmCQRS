using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Auth;
using BrahmCQRS.Application.DTOs.Email;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Domain.Exceptions;
using BrahmCQRS.Shared.Security;

namespace BrahmCQRS.Application.Services.Auth;

/// <summary>
/// Implementation of authentication user service.
/// </summary>
public class AuthUserService : IAuthUserService
{
    private readonly ICommandRepository<AuthUser> _userCommandRepo;
    private readonly IQueryRepository<AuthUser> _userQueryRepo;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthUserService(
        ICommandRepository<AuthUser> userCommandRepo,
        IQueryRepository<AuthUser> userQueryRepo,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userCommandRepo = userCommandRepo;
        _userQueryRepo = userQueryRepo;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<AuthUserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // Check if email exists
        var existingUser = await GetByEmailAsync(request.Email, cancellationToken);
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
            HasPassword = false,
            Activated = true,
            CreatedDate = DateTime.UtcNow
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

        return MapToDto(user);
    }

    public async Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetByEmailInternalAsync(email, cancellationToken);
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

        return MapToDto(user);
    }

    public async Task<AuthUserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetByEmailInternalAsync(email, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<AuthUserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userQueryRepo.GetByIdAsync(userId, false, cancellationToken);
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

    private async Task<AuthUser?> GetByEmailInternalAsync(string email, CancellationToken cancellationToken)
    {
        var users = await _userQueryRepo.ListAsync(false, cancellationToken);
        return users.FirstOrDefault(u => u.Email == email);
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
