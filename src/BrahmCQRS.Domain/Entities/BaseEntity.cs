using BrahmCQRS.Domain.Contracts.Common;

namespace BrahmCQRS.Domain.Entities;

/// <summary>
/// Base entity class for all domain entities with an integer identifier.
/// Provides common properties for auditing and soft delete functionality.
/// </summary>
public abstract class BaseEntity : BaseEntity<int>
{
}

/// <summary>
/// Generic base entity class for all domain entities.
/// Provides common properties for auditing and soft delete functionality.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class BaseEntity<TId> : IEntity<TId>, IAuditable, ISoftDeletable
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public TId Id { get; set; } = default!;

    /// <summary>
    /// Date and time when the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// User who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// User who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Indicates whether the entity is active (soft delete flag).
    /// </summary>
    public bool Activated { get; set; } = true;
}
