using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Auth;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Domain.Exceptions;
using BrahmCQRS.Shared.Security;

namespace BrahmCQRS.Application.Services.Auth;

/// <summary>
/// Implementation of session management service.
/// </summary>
public class SessionService : ISessionService
{
    private readonly IQueryRepository<AuthUser> _userQueryRepo;
    private readonly ICommandRepository<AuthSession> _sessionCommandRepo;
    private readonly ITokenService _tokenService;

    public SessionService(
        IQueryRepository<AuthUser> userQueryRepo,
        ICommandRepository<AuthSession> sessionCommandRepo,
        ITokenService tokenService)
    {
        _userQueryRepo = userQueryRepo;
        _sessionCommandRepo = sessionCommandRepo;
        _tokenService = tokenService;
    }

    public async Task<SessionDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var users = await _userQueryRepo.ListAsync(false, cancellationToken);
        var user = users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null || !user.Activated)
            throw new InvalidCredentialsException();

        // Verify email
        if (!user.EmailVerified)
            throw new EmailNotVerifiedException(user.Email);

        // Verify password
        if (!user.PasswordHash.ValidatePassword(request.Password))
            throw new InvalidCredentialsException();

        // Create user DTO
        var userDto = new AuthUserDto
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

        // Generate token
        var token = _tokenService.GenerateAccessToken(userDto);

        // Create session
        var session = new AuthSession
        {
            UserId = user.Id,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            CreatedDate = DateTime.UtcNow,
            Activated = true
        };

        await _sessionCommandRepo.CreateAsync(session, cancellationToken);

        return new SessionDto
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = session.ExpiresAt ?? DateTime.UtcNow.AddHours(8),
            User = userDto
        };
    }

    public async Task<SessionDto> RefreshTokenAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userQueryRepo.GetByIdAsync(userId, false, cancellationToken);

        if (user == null || !user.Activated)
            throw new EntityNotFoundException(nameof(AuthUser), userId);

        var userDto = new AuthUserDto
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

        var token = _tokenService.GenerateAccessToken(userDto);

        return new SessionDto
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            User = userDto
        };
    }

    public async Task LogoutAsync(string token, int userId, CancellationToken cancellationToken = default)
    {
        await _tokenService.RevokeTokenAsync(token, userId, "User logout", cancellationToken);
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return !await _tokenService.IsTokenRevokedAsync(token, cancellationToken);
    }
}
