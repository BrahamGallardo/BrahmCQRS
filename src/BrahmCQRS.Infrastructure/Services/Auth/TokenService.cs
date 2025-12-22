using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Auth;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Infrastructure.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BrahmCQRS.Infrastructure.Services.Auth;

/// <summary>
/// Implementation of JWT token service.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ICommandRepository<RevokedToken> _revokedTokenRepo;
    private readonly IQueryRepository<RevokedToken> _revokedTokenQueryRepo;

    public TokenService(
        JwtSettings jwtSettings,
        ICommandRepository<RevokedToken> revokedTokenRepo,
        IQueryRepository<RevokedToken> revokedTokenQueryRepo)
    {
        _jwtSettings = jwtSettings;
        _revokedTokenRepo = revokedTokenRepo;
        _revokedTokenQueryRepo = revokedTokenQueryRepo;
    }

    public string GenerateAccessToken(AuthUserDto user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim("role", user.RoleId.ToString()),
            new Claim("roleName", user.RoleName ?? "User")
        };

        var expirationMinutes = _jwtSettings.RoleTimeouts?.ContainsKey(user.RoleName ?? "") == true
            ? _jwtSettings.RoleTimeouts[user.RoleName!]
            : _jwtSettings.AccessTokenExpirationMinutes;

        return GenerateToken(claims, expirationMinutes);
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
            RevokedAt = DateTime.UtcNow,
            UserId = userId,
            Reason = reason,
            CreatedDate = DateTime.UtcNow,
            Activated = true
        };

        await _revokedTokenRepo.CreateAsync(revokedToken, cancellationToken);
    }

    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        var allTokens = await _revokedTokenQueryRepo.ListAsync(false, cancellationToken);
        return allTokens.Any(t => t.Token == token);
    }

    private string GenerateToken(Claim[] claims, int expirationMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
