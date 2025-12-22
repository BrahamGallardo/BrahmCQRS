using BrahmCQRS.Application.Common;
using BrahmCQRS.Domain.Contracts.Common;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Contracts.Specifications;
using BrahmCQRS.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BrahmCQRS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation for query operations (Read).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="QueryRepository{TEntity}"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class QueryRepository<TEntity>(DbContext context) : DisposeRepository<TEntity>(context), IQueryRepository<TEntity>
    where TEntity : class
{

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(
        int id,
        bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        var entity = await _context.Set<TEntity>()
            .FindAsync([id], cancellationToken);

        if (entity == null)
            return null;

        if (onlyActive && entity is ISoftDeletable softDeletable && !softDeletable.Activated)
            return null;

        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (onlyActive && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((ISoftDeletable)e).Activated);
        }

        return await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<TEntity>> ListAsync(
        bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (onlyActive && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((ISoftDeletable)e).Activated);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IPaginatedList<TEntity>> GetPaginatedAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification, applyPaging: false);

        var count = specification.IsPagingEnabled
            ? await query.CountAsync(cancellationToken)
            : 0;

        if (specification.IsPagingEnabled)
        {
            var skip = (specification.PageIndex - 1) * specification.PageSize;
            query = query.Skip(skip).Take(specification.PageSize);
        }

        var items = await query.ToListAsync(cancellationToken);

        return new PaginatedList<TEntity>(
            items,
            count,
            specification.PageIndex,
            specification.PageSize
        );
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification, applyPaging: false, applyOrdering: false);
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification, applyPaging: false, applyOrdering: false);
        return await query.AnyAsync(cancellationToken);
    }


    /// <inheritdoc/>
    public virtual async Task<bool> AnyAsync(
        bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (onlyActive && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((ISoftDeletable)e).Activated);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Applies the specification to the query.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="applyPaging">Whether to apply paging.</param>
    /// <param name="applyOrdering">Whether to apply ordering.</param>
    /// <returns>The query with the specification applied.</returns>
    protected virtual IQueryable<TEntity> ApplySpecification(
        ISpecification<TEntity> specification,
        bool applyPaging = true,
        bool applyOrdering = true)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        // Apply global query filter settings
        if (specification.IgnoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply disabled filter (using ISoftDeletable)
        if (!specification.IncludeDisabled && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((ISoftDeletable)e).Activated);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (applyOrdering)
        {
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }
        }

        // Apply AsNoTracking if specified
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}
