using BrahmCQRS.Application.DTOs.Queries;
using BrahmCQRS.Domain.Contracts.Common;
using BrahmCQRS.Domain.Contracts.Specifications;

namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Generic service contract for query operations (Read) in CQRS pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IQueryService<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="onlyActive">If true, only returns the entity if Activated is true. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity that matches the identifier, or null if not found.</returns>
    Task<TEntity?> GetByIdAsync(int id, bool onlyActive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities. Note: ISpecification works with active entities by default (Activated == true) unless IncludeDisabled is set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first entity that satisfies the specification, or null if not found.</returns>
    /// <remarks>
    /// ISpecification filters active entities by default unless IncludeDisabled is set.
    /// </remarks>
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity asynchronously without any specification.
    /// </summary>
    /// <param name="onlyActive">If true, only returns entities where Activated is true. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first entity, or null if not found.</returns>
    /// <remarks>
    /// Warning: Returns the first entity from the database without filtering.
    /// Consider using FirstOrDefaultAsync(ISpecification) for more controlled queries.
    /// </remarks>
    Task<TEntity?> FirstOrDefaultAsync(bool onlyActive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities asynchronously without any filtering.
    /// </summary>
    /// <param name="onlyActive">If true, only returns entities where Activated is true. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all entities.</returns>
    Task<List<TEntity>> GetListAsync(bool onlyActive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of entities that match the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities. Note: ISpecification works with active entities by default (Activated == true) unless IncludeDisabled is set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of entities that satisfy the specification.</returns>
    Task<List<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities that match the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of entities.</returns>
    Task<IPaginatedList<TEntity>> GetPaginatedAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities using pagination parameters asynchronously.
    /// </summary>
    /// <param name="parameters">The pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of entities.</returns>
    Task<IPaginatedList<TEntity>> GetPaginatedAsync(PaginationParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities that match the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total count of entities that satisfy the specification.</returns>
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity exists that matches the specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if any entity matches the specification; otherwise, false.</returns>
    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity exists asynchronously.
    /// </summary>
    /// <param name="onlyActive">If true, only checks for entities where Activated is true. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if any entity exists; otherwise, false.</returns>
    Task<bool> AnyAsync(bool onlyActive = false, CancellationToken cancellationToken = default);
}
