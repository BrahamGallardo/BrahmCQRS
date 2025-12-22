using System.Linq.Expressions;

namespace BrahmCQRS.Domain.Contracts.Specifications;

/// <summary>
/// Specification pattern contract for building flexible and reusable query criteria.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// By default, ISpecification filters only active entities (Activated == true).
/// To include disabled/inactive entities, set IncludeDisabled to true using ApplyIncludeDisabled() in your specification.
/// </remarks>
public interface ISpecification<TEntity>
{
    /// <summary>
    /// Gets the filter criteria expression.
    /// </summary>
    Expression<Func<TEntity, bool>> Criteria { get; }

    /// <summary>
    /// Gets the ascending order expression.
    /// </summary>
    Expression<Func<TEntity, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the descending order expression.
    /// </summary>
    Expression<Func<TEntity, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the list of include expressions for eager loading related entities.
    /// </summary>
    List<Expression<Func<TEntity, object>>> Includes { get; }

    /// <summary>
    /// Gets the list of include strings for eager loading related entities using string paths.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the page size for pagination.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the page index for pagination (1-based).
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    /// Gets a value indicating whether paging is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether to ignore global query filters.
    /// </summary>
    bool IgnoreQueryFilters { get; }

    /// <summary>
    /// Gets a value indicating whether to include disabled/inactive entities.
    /// </summary>
    /// <remarks>
    /// Default is false, which means only active entities (Activated == true) are returned.
    /// Set to true to include all entities regardless of their Activated status.
    /// </remarks>
    bool IncludeDisabled { get; }

    /// <summary>
    /// Gets a value indicating whether to use AsNoTracking for the query.
    /// </summary>
    /// <remarks>
    /// Default is true for read-only queries to improve performance.
    /// Set to false if you need to track entities for updates.
    /// </remarks>
    bool AsNoTracking { get; }
}
