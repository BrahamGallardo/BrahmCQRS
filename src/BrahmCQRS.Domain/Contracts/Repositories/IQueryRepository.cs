using BrahmCQRS.Domain.Contracts.Common;
using BrahmCQRS.Domain.Contracts.Specifications;

namespace BrahmCQRS.Domain.Contracts.Repositories;

/// <summary>
/// Repository contract for query operations (Read) in CQRS pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IQueryRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="onlyActive">If true, only returns the entity if Activated is true. Default is false.</param>
    /// <returns>The entity that matches the identifier, or null if not found.</returns>
    Task<TEntity?> GetByIdAsync(int id, bool onlyActive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities. Note: ISpecification works with active entities by default (Activated == true) unless IncludeDisabled is set.</param>
    /// <returns>The first entity that satisfies the specification, or null if not found.</returns>
    /// <remarks>
    /// Warning: Returns FirstOrDefault. This may return an element that you do not want.
    /// You need to specify the criteria very well.
    /// ISpecification filters active entities by default unless IncludeDisabled is set.
    /// </remarks>
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity asynchronously without any specification.
    /// </summary>
    /// <param name="onlyActive">If true, only returns entities where Activated is true. Default is false.</param>
    /// <returns>The first entity, or null if not found.</returns>
    /// <remarks>
    /// Warning: Returns FirstOrDefault. This may return any element from the database.
    /// Consider using a specification for more controlled queries.
    /// </remarks>
    Task<TEntity?> FirstOrDefaultAsync(bool onlyActive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities asynchronously without any filtering.
    /// </summary>
    /// <param name="onlyActive">If true, only returns entities where Activated is true. Default is false.</param>
    /// <returns>A list of all entities.</returns>
    /// <exception cref="NullReferenceException">Thrown when operation fails.</exception>
    /// <remarks>
    /// Warning: This method returns all elements in the database and can affect the runtime.
    /// </remarks>
    Task<List<TEntity>> ListAsync(bool onlyActive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of entities that match the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities. Note: ISpecification works with active entities by default (Activated == true) unless IncludeDisabled is set.</param>
    /// <returns>A list of entities that satisfy the specification.</returns>
    /// <exception cref="NullReferenceException">Thrown when operation fails.</exception>
    Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities that match the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities.</param>
    /// <returns>A paginated list of entities.</returns>
    /// <exception cref="NullReferenceException">Thrown when operation fails.</exception>
    Task<IPaginatedList<TEntity>> GetPaginatedAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities that match the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities.</param>
    /// <returns>The total count of entities that satisfy the specification.</returns>
    /// <exception cref="NullReferenceException">Thrown when operation fails.</exception>
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity exists that matches the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities.</param>
    /// <returns>True if any entity matches the specification; otherwise, false.</returns>
    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity exists asynchronously.
    /// </summary>
    /// <param name="onlyActive">If true, only checks for entities where Activated is true. Default is false.</param>
    /// <returns>True if any entity exists; otherwise, false.</returns>
    Task<bool> AnyAsync(bool onlyActive = false, CancellationToken cancellationToken = default);
}
