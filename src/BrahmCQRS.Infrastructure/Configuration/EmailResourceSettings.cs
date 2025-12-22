namespace BrahmCQRS.Infrastructure.Configuration;

/// <summary>
/// Email resource paths configuration (logos, images, etc.).
/// </summary>
public class EmailResourceSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Rutas";

    /// <summary>
    /// Path to logo directory.
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// Path to other email assets (future use).
    /// </summary>
    public string? Assets { get; set; }
}
