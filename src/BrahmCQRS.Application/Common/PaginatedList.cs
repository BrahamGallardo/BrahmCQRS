using BrahmCQRS.Domain.Contracts.Common;

namespace BrahmCQRS.Application.Common;

/// <summary>
/// Implementation of a paginated list with metadata.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PaginatedList<T> : IPaginatedList<T>
{
    /// <inheritdoc/>
    public IReadOnlyList<T> Items { get; }

    /// <inheritdoc/>
    public int PageIndex { get; }

    /// <inheritdoc/>
    public int PageSize { get; }

    /// <inheritdoc/>
    public int TotalPages { get; }

    /// <inheritdoc/>
    public int TotalCount { get; }

    /// <inheritdoc/>
    public T? TableFooter { get; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="count">The total count of items.</param>
    /// <param name="pageIndex">The current page index.</param>
    /// <param name="pageSize">The page size.</param>
    public PaginatedList(IReadOnlyList<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class with a table footer.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="count">The total count of items.</param>
    /// <param name="pageIndex">The current page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="tableFooter">The table footer data.</param>
    public PaginatedList(IReadOnlyList<T> items, int count, int pageIndex, int pageSize, T? tableFooter)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
        TableFooter = tableFooter;
    }
}
