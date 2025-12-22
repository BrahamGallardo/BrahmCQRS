namespace BrahmCQRS.Domain.Contracts.Common;

/// <summary>
/// Contract for entities that track creation and modification metadata.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; set; }
}
