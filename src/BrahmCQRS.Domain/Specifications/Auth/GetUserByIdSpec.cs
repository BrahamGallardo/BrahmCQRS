using BrahmCQRS.Domain.Entities;

namespace BrahmCQRS.Domain.Specifications.Auth;

/// <summary>
/// Specification to get a user by identifier, eagerly loading the related role.
/// </summary>
/// <remarks>
/// Prefer this specification over <c>IQueryRepository.GetByIdAsync</c> whenever the
/// role name is needed, because <c>GetByIdAsync</c> does not eager load navigations.
/// </remarks>
public class GetUserByIdSpec : BaseSpecification<AuthUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserByIdSpec"/> class.
    /// </summary>
    /// <param name="userId">The user identifier to match.</param>
    /// <param name="includeDisabled">
    /// If true, deactivated users (Activated == false) are also returned.
    /// </param>
    public GetUserByIdSpec(int userId, bool includeDisabled = false)
        : base(u => u.Id == userId)
    {
        AddInclude(u => u.Role!);
        ApplyIgnoreQueryFilters();

        if (includeDisabled)
            ApplyIncludeDisabled();
    }
}
