namespace BrahmCQRS.Domain.Exceptions;

/// <summary>
/// Exception thrown when user attempts to login without verified email.
/// </summary>
public class EmailNotVerifiedException : Exception
{
    public EmailNotVerifiedException()
        : base("Email address has not been verified.")
    {
    }

    public EmailNotVerifiedException(string email)
        : base($"Email '{email}' has not been verified.")
    {
    }

    public EmailNotVerifiedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
