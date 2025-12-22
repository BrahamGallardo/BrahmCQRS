using Microsoft.AspNetCore.JsonPatch;

namespace BrahmCQRS.Domain.Contracts.Repositories;

/// <summary>
/// Repository contract for command operations (CUD - Create, Update, Delete) in CQRS pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface ICommandRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Creates a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>The created entity.</returns>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk insert operation for multiple entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This is an optimized operation for inserting large amounts of data.
    /// Uses AddRange with a single SaveChanges call for better performance.
    /// </remarks>
    Task<IEnumerable<TEntity>> CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk update operation for multiple entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>The updated entities.</returns>
    /// <remarks>
    /// This is an optimized operation for updating large amounts of data.
    /// Uses UpdateRange with a single SaveChanges call for better performance.
    /// </remarks>
    Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Partially updates an entity using JSON Patch document asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="patchDocument">The JSON Patch document containing the changes.</param>
    /// <returns>The updated entity, or null if not found.</returns>
    Task<TEntity?> UpdatePatchAsync(int id, JsonPatchDocument patchDocument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an entity by setting Activated to false asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>True if the entity was soft deleted; otherwise, false.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes multiple entities by setting Activated to false asynchronously.
    /// </summary>
    /// <param name="ids">The entity identifiers.</param>
    /// <returns>The number of entities soft deleted.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<int> SoftDeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivates a soft-deleted entity by setting Activated to true asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>True if the entity was reactivated; otherwise, false.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<bool> ReactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivates multiple soft-deleted entities by setting Activated to true asynchronously.
    /// </summary>
    /// <param name="ids">The entity identifiers.</param>
    /// <returns>The number of entities reactivated.</returns>
    /// <remarks>
    /// Only works with entities that implement ISoftDeletable interface.
    /// </remarks>
    Task<int> ReactivateRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}
