namespace BrahmCQRS.Infrastructure.Configuration;

/// <summary>
/// JWT configuration settings.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key for signing tokens (minimum 32 characters).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (usually your app URL).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience (usually your app URL).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration in minutes (default: 480 = 8 hours).
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 480;

    /// <summary>
    /// Email confirmation token expiration in hours (default: 24).
    /// </summary>
    public int ConfirmationTokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// Password reset token expiration in hours (default: 2).
    /// </summary>
    public int PasswordResetTokenExpirationHours { get; set; } = 2;

    /// <summary>
    /// Role-specific timeout overrides (in minutes).
    /// Example: { "Customer": 120, "Admin": 480 }
    /// </summary>
    public Dictionary<string, int>? RoleTimeouts { get; set; }
}
