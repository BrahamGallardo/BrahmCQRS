using BrahmCQRS.Application.DTOs.Email;

namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Service contract for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously using the configured SMTP server.
    /// Automatically processes embedded resources (cid:xxx) if present in the body.
    /// </summary>
    /// <param name="emailDto">The email data transfer object containing recipient, subject, and body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully; otherwise, false.</returns>
    Task<bool> SendEmailAsync(EmailDto emailDto, CancellationToken cancellationToken = default);
}
