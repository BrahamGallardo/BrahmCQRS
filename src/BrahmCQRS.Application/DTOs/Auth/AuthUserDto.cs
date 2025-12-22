namespace BrahmCQRS.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for AuthUser.
/// </summary>
public class AuthUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool EmailVerified { get; set; }
    public bool HasPassword { get; set; }
    public bool Activated { get; set; }
    public DateTime CreatedDate { get; set; }
}
