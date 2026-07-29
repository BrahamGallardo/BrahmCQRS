using BrahmCQRS.Domain.Entities;

namespace BrahmCQRS.Domain.Specifications.Auth;

/// <summary>
/// Specification to select revoked-token records whose underlying tokens have already expired
/// and can therefore be purged from the blacklist.
/// </summary>
/// <remarks>
/// Records created before <c>ExpiresAt</c> was introduced (or from tokens that could not be
/// parsed) have a null expiration; those fall back to <paramref name="fallbackCutoffUtc"/>,
/// which the caller computes as "now minus the longest token lifetime the configuration
/// can possibly issue". That makes the fallback conservative but never premature.
/// </remarks>
public class GetPurgeableRevokedTokensSpec : BaseSpecification<RevokedToken>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPurgeableRevokedTokensSpec"/> class.
    /// </summary>
    /// <param name="utcNow">The current UTC time.</param>
    /// <param name="fallbackCutoffUtc">
    /// The revocation cutoff applied to records with an unknown expiration.
    /// </param>
    public GetPurgeableRevokedTokensSpec(DateTime utcNow, DateTime fallbackCutoffUtc)
        : base(t => (t.ExpiresAt != null && t.ExpiresAt < utcNow)
                 || (t.ExpiresAt == null && t.RevokedAt < fallbackCutoffUtc))
    {
    }
}
