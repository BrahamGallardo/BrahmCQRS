namespace BrahmCQRS.Infrastructure.Configuration;

/// <summary>
/// SMTP server configuration settings.
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Mail";

    /// <summary>
    /// SMTP server hostname or IP address.
    /// </summary>
    public required string SmtpServer { get; set; }

    /// <summary>
    /// SMTP server port (typically 587 for TLS, 465 for SSL, 25 for non-encrypted).
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// Sender email address (used for authentication and "From" field).
    /// </summary>
    public required string SenderEmail { get; set; }

    /// <summary>
    /// Sender email password or app password.
    /// </summary>
    public required string SenderPassword { get; set; }

    /// <summary>
    /// Optional sender display name.
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// Enable SSL/TLS encryption.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Enable test mode (prevents actual email sending for testing/staging environments).
    /// </summary>
    public bool EnableTestMode { get; set; } = false;
}
