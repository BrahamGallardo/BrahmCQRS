namespace BrahmCQRS.Application.DTOs.Auth;

/// <summary>
/// User registration request.
/// </summary>
public class RegisterRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; } = 2; // Default to User role
}
