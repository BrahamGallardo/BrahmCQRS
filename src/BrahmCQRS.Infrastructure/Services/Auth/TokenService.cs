using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Auth;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Domain.Specifications.Auth;
using BrahmCQRS.Infrastructure.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BrahmCQRS.Infrastructure.Services.Auth;

/// <summary>
/// Implementation of JWT token service.
/// </summary>
/// <remarks>
/// All security timestamps handled by this service are UTC. Audit fields
/// (CreatedDate, UpdatedDate) are populated by BaseDbContext in server time.
/// </remarks>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ICommandRepository<RevokedToken> _revokedTokenRepo;
    private readonly IQueryRepository<RevokedToken> _revokedTokenQueryRepo;
    private readonly ITimeProvider _timeProvider;

    public TokenService(
        JwtSettings jwtSettings,
        ICommandRepository<RevokedToken> revokedTokenRepo,
        IQueryRepository<RevokedToken> revokedTokenQueryRepo,
        ITimeProvider timeProvider)
    {
        _jwtSettings = jwtSettings;
        _revokedTokenRepo = revokedTokenRepo;
        _revokedTokenQueryRepo = revokedTokenQueryRepo;
        _timeProvider = timeProvider;
    }

    public string GenerateAccessToken(AuthUserDto user)
    {
        var roleName = user.RoleName ?? "User";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim("role", user.RoleId.ToString()),
            new Claim("roleName", roleName),

            // Standard role claim so that [Authorize(Roles = "...")] works out of the box.
            // The legacy "role" (role id) and "roleName" claims are kept for compatibility.
            new Claim(ClaimTypes.Role, roleName)
        };

        return GenerateToken(claims, (int)GetAccessTokenLifetime(user.RoleName).TotalMinutes);
    }

    public string GenerateConfirmationToken(int userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("type", "email_confirmation")
        };

        return GenerateToken(claims, _jwtSettings.ConfirmationTokenExpirationHours * 60);
    }

    public string GeneratePasswordResetToken(int userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("type", "password_reset")
        };

        return GenerateToken(claims, _jwtSettings.PasswordResetTokenExpirationHours * 60);
    }

    public string GenerateSetupToken(int userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("type", "password_setup")
        };

        return GenerateToken(claims, _jwtSettings.PasswordResetTokenExpirationHours * 60);
    }

    public Task<int?> ValidateAndExtractUserIdAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Task.FromResult(int.TryParse(userIdClaim, out var userId) ? (int?)userId : null);
        }
        catch
        {
            return Task.FromResult<int?>(null);
        }
    }

    public async Task RevokeTokenAsync(string token, int? userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var revokedToken = new RevokedToken
        {
            Token = token,
            RevokedAt = _timeProvider.GetUtcNow(),
            ExpiresAt = TryReadExpiration(token),
            UserId = userId,
            Reason = reason
            // CreatedDate / Activated are set automatically by BaseDbContext.
        };

        await _revokedTokenRepo.CreateAsync(revokedToken, cancellationToken);

        // Opportunistic housekeeping: revocations are rare (logout, password change),
        // so this never runs on the hot authenticated-request path. A failure here must
        // not prevent the revocation itself from taking effect.
        try
        {
            await PurgeExpiredRevokedTokensAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Purging is best-effort maintenance; swallow and let the next revocation retry.
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        // Runs on every authenticated request through JwtBearerEvents.OnTokenValidated,
        // so it must translate to a single EXISTS query instead of loading the table.
        return await _revokedTokenQueryRepo.AnyAsync(new GetRevokedTokenSpec(token), cancellationToken);
    }

    /// <inheritdoc/>
    public TimeSpan GetAccessTokenLifetime(string? roleName)
    {
        var minutes = _jwtSettings.AccessTokenExpirationMinutes;

        if (!string.IsNullOrEmpty(roleName)
            && _jwtSettings.RoleTimeouts?.TryGetValue(roleName, out var roleMinutes) == true)
        {
            minutes = roleMinutes;
        }

        return TimeSpan.FromMinutes(minutes);
    }

    /// <inheritdoc/>
    public async Task<int> PurgeExpiredRevokedTokensAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = _timeProvider.GetUtcNow();
        var fallbackCutoff = utcNow - GetLongestPossibleTokenLifetime();

        var purgeable = await _revokedTokenQueryRepo.ListAsync(
            new GetPurgeableRevokedTokensSpec(utcNow, fallbackCutoff),
            cancellationToken);

        if (purgeable.Count == 0)
            return 0;

        return await _revokedTokenRepo.SoftDeleteRangeAsync(
            purgeable.Select(t => t.Id),
            cancellationToken);
    }

    /// <summary>
    /// Reads the expiration from a token without validating its signature.
    /// </summary>
    /// <param name="token">The raw token string.</param>
    /// <returns>The UTC expiration, or null when the token is not a readable JWT.</returns>
    /// <remarks>
    /// The signature is irrelevant here: the value is only used to decide when the
    /// blacklist entry can be purged, never to grant access.
    /// </remarks>
    private static DateTime? TryReadExpiration(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                return null;

            var validTo = handler.ReadJwtToken(token).ValidTo;

            return validTo == DateTime.MinValue ? null : validTo;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the longest lifetime this configuration can possibly issue.
    /// </summary>
    /// <returns>The longest configured token lifetime.</returns>
    private TimeSpan GetLongestPossibleTokenLifetime()
    {
        var minutes = _jwtSettings.AccessTokenExpirationMinutes;

        if (_jwtSettings.RoleTimeouts is { Count: > 0 })
            minutes = Math.Max(minutes, _jwtSettings.RoleTimeouts.Values.Max());

        minutes = Math.Max(minutes, _jwtSettings.ConfirmationTokenExpirationHours * 60);
        minutes = Math.Max(minutes, _jwtSettings.PasswordResetTokenExpirationHours * 60);

        return TimeSpan.FromMinutes(minutes);
    }

    private string GenerateToken(Claim[] claims, int expirationMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var utcNow = _timeProvider.GetUtcNow();

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: utcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
