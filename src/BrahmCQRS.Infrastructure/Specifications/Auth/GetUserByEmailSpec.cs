using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Domain.Specifications;

namespace BrahmCQRS.Infrastructure.Specifications.Auth;

/// <summary>
/// Specification to get user by email including role.
/// </summary>
public class GetUserByEmailSpec : BaseSpecification<AuthUser>
{
    public GetUserByEmailSpec(string email)
        : base(u => u.Email == email)
    {
        AddInclude(u => u.Role!);
        IgnoreQueryFilters = true; // Include soft-deleted
    }
}
