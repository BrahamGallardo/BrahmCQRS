namespace BrahmCQRS.Application.DTOs.Queries;

/// <summary>
/// Data transfer object for pagination and filtering parameters.
/// </summary>
public class PaginationParameters
{
    /// <summary>
    /// Gets or sets the search term for filtering.
    /// </summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sort direction ("asc" or "desc").
    /// </summary>
    public string SortDirection { get; set; } = "desc";

    /// <summary>
    /// Gets or sets the field name to sort by.
    /// </summary>
    public string SortBy { get; set; } = "id";

    /// <summary>
    /// Gets or sets a value indicating whether to include disabled/inactive entities.
    /// </summary>
    public bool IncludeDisabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the page index (1-based).
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 15;
}
