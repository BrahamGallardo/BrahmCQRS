namespace BrahmCQRS.Application.DTOs.Auth;

/// <summary>
/// Change password request.
/// </summary>
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
