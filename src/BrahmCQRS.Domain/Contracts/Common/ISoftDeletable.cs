namespace BrahmCQRS.Domain.Contracts.Common;

/// <summary>
/// Contract for entities that support soft delete functionality.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is active.
    /// When false, the entity is considered soft-deleted.
    /// </summary>
    bool Activated { get; set; }
}
