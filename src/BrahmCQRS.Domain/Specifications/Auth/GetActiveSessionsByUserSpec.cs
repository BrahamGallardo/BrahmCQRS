using BrahmCQRS.Domain.Entities;

namespace BrahmCQRS.Domain.Specifications.Auth;

/// <summary>
/// Specification to get every session of a user that is still flagged as active,
/// regardless of whether it has already expired.
/// </summary>
/// <remarks>
/// Used to close sessions on logout and on password change or reset.
/// </remarks>
public class GetActiveSessionsByUserSpec : BaseSpecification<AuthSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetActiveSessionsByUserSpec"/> class.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    public GetActiveSessionsByUserSpec(int userId)
        : base(s => s.UserId == userId && s.IsActive)
    {
    }
}
