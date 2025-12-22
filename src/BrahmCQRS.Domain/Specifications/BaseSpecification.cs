using System.Linq.Expressions;
using BrahmCQRS.Domain.Contracts.Specifications;

namespace BrahmCQRS.Domain.Specifications;

/// <summary>
/// Base implementation of the Specification pattern for building flexible query criteria.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// By default, specifications filter only active entities (Activated == true).
/// To include disabled/inactive entities, call ApplyIncludeDisabled() in your derived specification constructor.
/// </remarks>
public abstract class BaseSpecification<TEntity> : ISpecification<TEntity>
{
    /// <summary>
    /// Initializes a new instance with no criteria (matches all entities).
    /// </summary>
    protected BaseSpecification()
    {
        Criteria = _ => true;
    }

    /// <summary>
    /// Initializes a new instance with the specified criteria.
    /// </summary>
    /// <param name="criteria">The filter criteria expression.</param>
    protected BaseSpecification(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <inheritdoc/>
    public Expression<Func<TEntity, bool>> Criteria { get; }

    /// <inheritdoc/>
    public List<Expression<Func<TEntity, object>>> Includes { get; } = [];

    /// <inheritdoc/>
    public Expression<Func<TEntity, object>>? OrderBy { get; protected set; }

    /// <inheritdoc/>
    public Expression<Func<TEntity, object>>? OrderByDescending { get; protected set; }

    /// <inheritdoc/>
    public List<string> IncludeStrings { get; } = [];

    /// <inheritdoc/>
    public int PageSize { get; protected set; } = 15;

    /// <inheritdoc/>
    public int PageIndex { get; protected set; } = 1;

    /// <inheritdoc/>
    public bool IsPagingEnabled { get; protected set; } = false;

    /// <inheritdoc/>
    public bool IgnoreQueryFilters { get; protected set; } = false;

    /// <inheritdoc/>
    public bool IncludeDisabled { get; protected set; } = false;

    /// <inheritdoc/>
    public bool AsNoTracking { get; protected set; } = true;

    /// <summary>
    /// Adds an include expression for eager loading related entities.
    /// </summary>
    /// <param name="includeExpression">The include expression.</param>
    protected virtual void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include for eager loading related entities.
    /// Allows for including children of children, e.g., "Basket.Items.Product".
    /// </summary>
    /// <param name="includeString">The include string path.</param>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Sets the ascending order expression.
    /// </summary>
    /// <param name="orderByExpression">The order by expression.</param>
    protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Sets the descending order expression.
    /// </summary>
    /// <param name="orderByDescExpression">The order by descending expression.</param>
    protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Enables pagination with the specified page size and index.
    /// </summary>
    /// <param name="pageIndex">The page index (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    protected void ApplyPaging(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Enables inclusion of disabled/inactive entities.
    /// </summary>
    /// <remarks>
    /// By default, specifications only return active entities (Activated == true).
    /// Call this method in your specification constructor to include all entities regardless of their Activated status.
    /// </remarks>
    protected void ApplyIncludeDisabled()
    {
        IncludeDisabled = true;
    }

    /// <summary>
    /// Enables ignoring of global query filters.
    /// </summary>
    protected void ApplyIgnoreQueryFilters()
    {
        IgnoreQueryFilters = true;
    }

    /// <summary>
    /// Disables AsNoTracking for the query.
    /// </summary>
    /// <remarks>
    /// By default, specifications use AsNoTracking for read-only queries.
    /// Call this method if you need to track entities for updates.
    /// </remarks>
    protected void DisableAsNoTracking()
    {
        AsNoTracking = false;
    }
}
