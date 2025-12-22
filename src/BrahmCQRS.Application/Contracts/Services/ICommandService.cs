using Microsoft.AspNetCore.JsonPatch;

namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Generic service contract for command operations (Create, Update, Delete) in CQRS pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface ICommandService<TEntity> where TEntity : class
{
    /// <summary>
    /// Creates a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created entity.</returns>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple new entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created entities.</returns>
    Task<IEnumerable<TEntity>> CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entity.</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple existing entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entities.</returns>
    Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Partially updates an entity using JSON Patch document asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="patchDocument">The JSON Patch document containing the changes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entity, or null if not found.</returns>
    Task<TEntity?> PatchAsync(int id, JsonPatchDocument patchDocument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an entity by setting Activated to false asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity was soft deleted; otherwise, false.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes multiple entities by setting Activated to false asynchronously.
    /// </summary>
    /// <param name="ids">The entity identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entities soft deleted.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<int> SoftDeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivates a soft-deleted entity by setting Activated to true asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity was reactivated; otherwise, false.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<bool> ReactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivates multiple soft-deleted entities by setting Activated to true asynchronously.
    /// </summary>
    /// <param name="ids">The entity identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entities reactivated.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<int> ReactivateRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}
