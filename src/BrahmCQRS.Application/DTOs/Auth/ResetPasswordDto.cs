namespace BrahmCQRS.Application.DTOs.Auth;

/// <summary>
/// Reset password request with token.
/// </summary>
public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
