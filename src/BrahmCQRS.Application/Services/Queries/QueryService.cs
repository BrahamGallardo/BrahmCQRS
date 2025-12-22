using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Queries;
using BrahmCQRS.Domain.Contracts.Common;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Contracts.Specifications;

namespace BrahmCQRS.Application.Services.Queries;

/// <summary>
/// Generic implementation of query service for CQRS pattern.
/// Provides operations for reading and querying entities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="QueryService{TEntity}"/> class.
/// </remarks>
/// <param name="repository">The query repository.</param>
public class QueryService<TEntity>(IQueryRepository<TEntity> repository) : IQueryService<TEntity> where TEntity : class
{
    private readonly IQueryRepository<TEntity> _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(int id, bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        return await _repository.GetByIdAsync(id, onlyActive, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await _repository.FirstOrDefaultAsync(specification, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        return await _repository.FirstOrDefaultAsync(onlyActive, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<TEntity>> GetListAsync(bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        return await _repository.ListAsync(onlyActive, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await _repository.ListAsync(specification, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IPaginatedList<TEntity>> GetPaginatedAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await _repository.GetPaginatedAsync(specification, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IPaginatedList<TEntity>> GetPaginatedAsync(PaginationParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        // Create a specification from pagination parameters
        var specification = CreateSpecificationFromParameters(parameters);
        return await _repository.GetPaginatedAsync(specification, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await _repository.CountAsync(specification, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await _repository.AnyAsync(specification, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> AnyAsync(bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        return await _repository.AnyAsync(onlyActive, cancellationToken);
    }

    /// <summary>
    /// Creates a specification from pagination parameters.
    /// This method can be overridden in derived classes to provide custom specification logic.
    /// </summary>
    /// <param name="parameters">The pagination parameters.</param>
    /// <returns>A specification for the entity.</returns>
    protected virtual ISpecification<TEntity> CreateSpecificationFromParameters(PaginationParameters parameters)
    {
        // This is a base implementation that needs to be overridden in derived classes
        // or you can inject a specification factory
        throw new NotImplementedException(
            "CreateSpecificationFromParameters must be implemented in derived classes or use GetPaginatedAsync with ISpecification directly.");
    }
}
