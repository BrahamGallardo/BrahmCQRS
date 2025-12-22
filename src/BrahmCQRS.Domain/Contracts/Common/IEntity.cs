namespace BrahmCQRS.Domain.Contracts.Common;

/// <summary>
/// Base contract for entities with a generic identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IEntity<TId>
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    TId Id { get; set; }
}
