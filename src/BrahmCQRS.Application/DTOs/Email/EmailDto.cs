namespace BrahmCQRS.Application.DTOs.Email;

/// <summary>
/// Data transfer object for email messages.
/// </summary>
public class EmailDto
{
    /// <summary>
    /// Primary recipient email address.
    /// </summary>
    public required string To { get; set; }

    /// <summary>
    /// Carbon copy recipients (optional).
    /// </summary>
    public string? Cc { get; set; }

    /// <summary>
    /// Blind carbon copy recipients (optional).
    /// </summary>
    public string? Bcc { get; set; }

    /// <summary>
    /// Email subject line.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// Email body content (can be HTML or plain text).
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Indicates whether the body is HTML formatted.
    /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// Optional embedded resources (key: cid identifier, value: file path).
    /// Example: { "logoId", "C:/path/to/logo.png" }
    /// </summary>
    public Dictionary<string, string>? EmbeddedResources { get; set; }
}
