using Microsoft.EntityFrameworkCore;

namespace BrahmCQRS.Infrastructure.Persistence.Repositories.Base;

/// <summary>
/// Base repository class that provides access to the database context.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Note: This class does NOT dispose the DbContext because it is managed by the DI container.
/// Disposing it here would cause ObjectDisposedException when other repositories share the same context.
/// </remarks>
public abstract class DisposeRepository<TEntity> : IDisposable, IAsyncDisposable
    where TEntity : class
{
    /// <summary>
    /// The database context instance.
    /// </summary>
    protected readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposeRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    protected DisposeRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Disposes the repository.
    /// </summary>
    /// <remarks>
    /// The DbContext is NOT disposed here as it is managed by the DI container.
    /// </remarks>
    public virtual void Dispose()
    {
        // DbContext lifetime is managed by DI container
        // Do not dispose it here to avoid ObjectDisposedException in other repositories
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes the repository.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    /// <remarks>
    /// The DbContext is NOT disposed here as it is managed by the DI container.
    /// </remarks>
    public virtual ValueTask DisposeAsync()
    {
        // DbContext lifetime is managed by DI container
        // Do not dispose it here to avoid ObjectDisposedException in other repositories
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
