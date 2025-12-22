using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Email;
using BrahmCQRS.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BrahmCQRS.Infrastructure.Services.Email;

/// <summary>
/// Email service implementation using MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly EmailResourceSettings _resourceSettings;

    public EmailService(
        IOptions<SmtpSettings> smtpSettings,
        IOptions<EmailResourceSettings> resourceSettings)
    {
        _smtpSettings = smtpSettings.Value;
        _resourceSettings = resourceSettings.Value;
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(EmailDto emailDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test mode bypass - prevents sending emails in test/staging environments
            if (_smtpSettings.EnableTestMode)
            {
                return true;
            }

            // Build the message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName ?? "System", _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", emailDto.To));

            // Add Cc recipients if present
            if (!string.IsNullOrWhiteSpace(emailDto.Cc))
            {
                foreach (var cc in emailDto.Cc.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Cc.Add(new MailboxAddress("", cc.Trim()));
                }
            }

            // Add Bcc recipients if present
            if (!string.IsNullOrWhiteSpace(emailDto.Bcc))
            {
                foreach (var bcc in emailDto.Bcc.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Bcc.Add(new MailboxAddress("", bcc.Trim()));
                }
            }

            message.Subject = emailDto.Subject;

            // Build body with embedded resources
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailDto.IsHtml ? emailDto.Body : null,
                TextBody = !emailDto.IsHtml ? emailDto.Body : null
            };

            // Process embedded resources automatically
            ProcessEmbeddedResources(emailDto.Body, bodyBuilder);

            message.Body = bodyBuilder.ToMessageBody();

            // Send the email
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _smtpSettings.SmtpServer,
                _smtpSettings.SmtpPort,
                _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            await client.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            // Consider using ILogger here instead of throwing
            throw new Exception($"Failed to send email to {emailDto.To}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Processes embedded resources in the email body (e.g., cid:logoId).
    /// </summary>
    private void ProcessEmbeddedResources(string body, BodyBuilder bodyBuilder)
    {
        // Process logoId automatically
        if (body.Contains("cid:logoId") && !string.IsNullOrWhiteSpace(_resourceSettings.Logo))
        {
            var logoPath = Path.Combine(_resourceSettings.Logo, "image.png");
            if (File.Exists(logoPath))
            {
                var linkedResource = new MimePart("image", "png")
                {
                    Content = new MimeContent(File.OpenRead(logoPath)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    ContentId = "logoId"
                };
                bodyBuilder.LinkedResources.Add(linkedResource);
            }
        }

        // Future: Add more embedded resources here
        // Example: cid:bannerImage, cid:footerLogo, etc.
    }
}
