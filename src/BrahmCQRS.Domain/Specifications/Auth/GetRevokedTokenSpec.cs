using BrahmCQRS.Domain.Entities;

namespace BrahmCQRS.Domain.Specifications.Auth;

/// <summary>
/// Specification to check whether a specific token has been revoked.
/// </summary>
/// <remarks>
/// Purged records are soft deleted (Activated == false) and are intentionally excluded
/// by this specification: purging only ever targets tokens that already expired, and
/// those are rejected earlier by JWT lifetime validation.
/// </remarks>
public class GetRevokedTokenSpec : BaseSpecification<RevokedToken>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRevokedTokenSpec"/> class.
    /// </summary>
    /// <param name="token">The raw token string to look up.</param>
    public GetRevokedTokenSpec(string token)
        : base(t => t.Token == token)
    {
        ApplyIgnoreQueryFilters();
    }
}
