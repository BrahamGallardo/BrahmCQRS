namespace BrahmCQRS.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for authentication session.
/// </summary>
public class SessionDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthUserDto User { get; set; } = null!;
}
