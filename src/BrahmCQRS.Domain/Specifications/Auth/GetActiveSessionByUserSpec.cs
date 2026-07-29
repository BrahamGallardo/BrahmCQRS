using BrahmCQRS.Domain.Entities;

namespace BrahmCQRS.Domain.Specifications.Auth;

/// <summary>
/// Specification to get the most recent session of a user that is active and not yet expired.
/// </summary>
/// <remarks>
/// Sessions with a null expiration are treated as non-expiring so that records created
/// before expiration alignment was enforced remain usable.
/// </remarks>
public class GetActiveSessionByUserSpec : BaseSpecification<AuthSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetActiveSessionByUserSpec"/> class.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="utcNow">The current UTC time used to discard expired sessions.</param>
    public GetActiveSessionByUserSpec(int userId, DateTime utcNow)
        : base(s => s.UserId == userId
                 && s.IsActive
                 && (s.ExpiresAt == null || s.ExpiresAt > utcNow))
    {
        AddOrderByDescending(s => s.Id);
    }
}
