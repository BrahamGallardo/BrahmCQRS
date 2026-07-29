using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Auth;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Domain.Exceptions;
using BrahmCQRS.Domain.Specifications.Auth;
using BrahmCQRS.Shared.Security;

namespace BrahmCQRS.Application.Services.Auth;

/// <summary>
/// Implementation of session management service.
/// </summary>
/// <remarks>
/// Session lifetimes are stored in UTC and aligned with the access token lifetime
/// reported by <see cref="ITokenService.GetAccessTokenLifetime"/>, so role-specific
/// timeouts apply to both the token and its session.
/// </remarks>
public class SessionService : ISessionService
{
    private readonly IQueryRepository<AuthUser> _userQueryRepo;
    private readonly ICommandRepository<AuthSession> _sessionCommandRepo;
    private readonly IQueryRepository<AuthSession> _sessionQueryRepo;
    private readonly ITokenService _tokenService;
    private readonly ITimeProvider _timeProvider;

    public SessionService(
        IQueryRepository<AuthUser> userQueryRepo,
        ICommandRepository<AuthSession> sessionCommandRepo,
        IQueryRepository<AuthSession> sessionQueryRepo,
        ITokenService tokenService,
        ITimeProvider timeProvider)
    {
        _userQueryRepo = userQueryRepo;
        _sessionCommandRepo = sessionCommandRepo;
        _sessionQueryRepo = sessionQueryRepo;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<SessionDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Single indexed query including the role, so RoleName is populated and
        // JwtSettings.RoleTimeouts can actually be applied.
        var user = await _userQueryRepo.FirstOrDefaultAsync(
            new GetUserByEmailSpec(request.Email),
            cancellationToken);

        if (user == null || !user.Activated)
            throw new InvalidCredentialsException();

        // Verify email
        if (!user.EmailVerified)
            throw new EmailNotVerifiedException(user.Email);

        // Verify password
        if (!user.PasswordHash.ValidatePassword(request.Password))
            throw new InvalidCredentialsException();

        var userDto = MapToDto(user);

        // Generate token
        var token = _tokenService.GenerateAccessToken(userDto);

        var utcNow = _timeProvider.GetUtcNow();
        var expiresAt = utcNow + _tokenService.GetAccessTokenLifetime(userDto.RoleName);

        // Create session
        var session = new AuthSession
        {
            UserId = user.Id,
            IsActive = true,
            ExpiresAt = expiresAt
            // CreatedDate / Activated are set automatically by BaseDbContext.
        };

        await _sessionCommandRepo.CreateAsync(session, cancellationToken);

        return new SessionDto
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }

    /// <inheritdoc/>
    public async Task<SessionDto> RefreshTokenAsync(int userId, CancellationToken cancellationToken = default)
    {
        var utcNow = _timeProvider.GetUtcNow();

        // A refresh is only legitimate while the user still holds a live session.
        var session = await _sessionQueryRepo.FirstOrDefaultAsync(
            new GetActiveSessionByUserSpec(userId, utcNow),
            cancellationToken);

        if (session == null)
            throw new InvalidCredentialsException("No active session found for this user");

        // Including the role keeps RoleName populated across refreshes.
        var user = await _userQueryRepo.FirstOrDefaultAsync(
            new GetUserByIdSpec(userId),
            cancellationToken);

        if (user == null || !user.Activated)
            throw new EntityNotFoundException(nameof(AuthUser), userId);

        var userDto = MapToDto(user);
        var token = _tokenService.GenerateAccessToken(userDto);
        var expiresAt = utcNow + _tokenService.GetAccessTokenLifetime(userDto.RoleName);

        // Slide the session forward so it stays aligned with the token just issued.
        session.ExpiresAt = expiresAt;
        await _sessionCommandRepo.UpdateAsync(session, cancellationToken);

        return new SessionDto
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }

    /// <inheritdoc/>
    public async Task LogoutAsync(string token, int userId, CancellationToken cancellationToken = default)
    {
        await _tokenService.RevokeTokenAsync(token, userId, "User logout", cancellationToken);

        // Every session of the user is closed, not just the current device: AuthSession
        // stores neither the token nor a device identifier, so this is the safe default.
        await CloseActiveSessionsAsync(userId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateTokenAsync(string token, int userId, CancellationToken cancellationToken = default)
    {
        if (await _tokenService.IsTokenRevokedAsync(token, cancellationToken))
            return false;

        return await _sessionQueryRepo.AnyAsync(
            new GetActiveSessionByUserSpec(userId, _timeProvider.GetUtcNow()),
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

    /// <summary>
    /// Maps a user entity to its data transfer object.
    /// </summary>
    /// <param name="user">The user entity, with the Role navigation loaded.</param>
    /// <returns>The mapped data transfer object.</returns>
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
