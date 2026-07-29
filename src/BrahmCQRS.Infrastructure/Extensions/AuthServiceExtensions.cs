using System.Text;
using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.Services.Auth;
using BrahmCQRS.Infrastructure.Configuration;
using BrahmCQRS.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace BrahmCQRS.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering BrahmCQRS authentication services.
/// </summary>
public static class AuthServiceExtensions
{
    /// <summary>
    /// Adds BrahmCQRS authentication module with JWT support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configureJwt">Optional JWT configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBrahmAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtSettings>? configureJwt = null)
    {
        // Bind JWT settings from appsettings
        var jwtSettings = new JwtSettings();
        configuration.GetSection("BrahmCQRS:Auth:JWT").Bind(jwtSettings);
        configureJwt?.Invoke(jwtSettings);

        // Validate settings
        if (string.IsNullOrEmpty(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");

        if (string.IsNullOrEmpty(jwtSettings.Issuer))
            throw new InvalidOperationException("JWT Issuer is required");

        if (string.IsNullOrEmpty(jwtSettings.Audience))
            throw new InvalidOperationException("JWT Audience is required");

        services.AddSingleton(jwtSettings);

        // TokenService and SessionService depend on ITimeProvider. TryAdd keeps
        // AddBrahmCQRSCore (or a custom registration) authoritative when present.
        services.TryAddSingleton<ITimeProvider>(_ => new Services.TimeProvider());

        // Register auth services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAuthUserService, AuthUserService>();
        services.AddScoped<ITokenService, TokenService>();

        // Configure JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Validate token revocation
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokenService = context.HttpContext.RequestServices
                            .GetRequiredService<ITokenService>();

                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            var token = authHeader.Substring("Bearer ".Length);

                            if (await tokenService.IsTokenRevokedAsync(token))
                            {
                                context.Fail("Token has been revoked");
                            }
                        }
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
