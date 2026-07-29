using BrahmCQRS.Domain.Entities;

namespace BrahmCQRS.Domain.Specifications.Auth;

/// <summary>
/// Specification to get a user by email address, eagerly loading the related role.
/// </summary>
/// <remarks>
/// Loading the role is required so that role-based claims and role-specific token
/// timeouts can be resolved without an extra round trip.
/// </remarks>
public class GetUserByEmailSpec : BaseSpecification<AuthUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserByEmailSpec"/> class.
    /// </summary>
    /// <param name="email">The email address to match.</param>
    /// <param name="includeDisabled">
    /// If true, deactivated users (Activated == false) are also returned.
    /// Use true for uniqueness checks during registration; use false for authentication flows.
    /// </param>
    public GetUserByEmailSpec(string email, bool includeDisabled = false)
        : base(u => u.Email == email)
    {
        AddInclude(u => u.Role!);
        ApplyIgnoreQueryFilters();

        if (includeDisabled)
            ApplyIncludeDisabled();
    }
}
