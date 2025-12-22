namespace BrahmCQRS.Application.DTOs.Auth;

/// <summary>
/// Email confirmation request.
/// </summary>
public class ConfirmEmailDto
{
    public string Token { get; set; } = string.Empty;
}
