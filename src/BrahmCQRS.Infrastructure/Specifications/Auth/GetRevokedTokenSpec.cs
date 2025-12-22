using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Domain.Specifications;

namespace BrahmCQRS.Infrastructure.Specifications.Auth;

/// <summary>
/// Specification to check if a token is revoked.
/// </summary>
public class GetRevokedTokenSpec : BaseSpecification<RevokedToken>
{
    public GetRevokedTokenSpec(string token)
        : base(t => t.Token == token)
    {
        IgnoreQueryFilters = true;
    }
}
