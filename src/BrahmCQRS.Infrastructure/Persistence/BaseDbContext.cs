using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrahmCQRS.Infrastructure.Persistence;

/// <summary>
/// Base database context with automatic audit field population.
/// Automatically sets CreatedBy, CreatedDate, UpdatedBy, and UpdatedDate for entities inheriting from BaseEntity.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
    /// </summary>
    /// <param name="currentUserService">The current user service for retrieving authenticated user information.</param>
    /// <param name="timeProvider">The time provider for server time operations.</param>
    /// <param name="options">The database context options.</param>
    protected BaseDbContext(
        ICurrentUserService currentUserService,
        ITimeProvider timeProvider,
        DbContextOptions options)
        : base(options)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// Automatically populates audit fields for entities inheriting from BaseEntity.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    // No audit updates needed for deletions
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedDate = _timeProvider.GetServerTime();
                    entry.Entity.UpdatedBy = _currentUserService.GetCurrentUserId();
                    break;

                case EntityState.Added:
                    entry.Entity.CreatedDate = _timeProvider.GetServerTime();
                    entry.Entity.CreatedBy = _currentUserService.GetCurrentUserId();
                    entry.Entity.Activated = true;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
