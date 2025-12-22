namespace BrahmCQRS.Domain.Contracts.Common;

/// <summary>
/// Contract for paginated list results.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public interface IPaginatedList<T>
{
    /// <summary>
    /// Gets the items in the current page.
    /// </summary>
    IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Gets the current page index (1-based).
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Gets the total count of items across all pages.
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Gets the table footer data (if applicable).
    /// </summary>
    T? TableFooter { get; }
}
