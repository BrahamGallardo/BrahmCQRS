using BrahmCQRS.Domain.Contracts.Common;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Infrastructure.Persistence.Repositories.Base;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace BrahmCQRS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation for command operations (Create, Update, Delete).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandRepository{TEntity}"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class CommandRepository<TEntity>(DbContext context) : DisposeRepository<TEntity>(context), ICommandRepository<TEntity>
    where TEntity : class
{

    /// <inheritdoc/>
    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();
        if (!entityList.Any())
            throw new ArgumentException("Entities collection cannot be empty.", nameof(entities));

        await _context.Set<TEntity>().AddRangeAsync(entityList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entityList;
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.Set<TEntity>().Attach(entity);
        }
        entry.State = EntityState.Modified;

        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();
        if (!entityList.Any())
            throw new ArgumentException("Entities collection cannot be empty.", nameof(entities));

        _context.Set<TEntity>().UpdateRange(entityList);
        await _context.SaveChangesAsync(cancellationToken);

        return entityList;
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> UpdatePatchAsync(int id, JsonPatchDocument patchDocument, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        ArgumentNullException.ThrowIfNull(patchDocument);

        // Validate that patch doesn't try to modify protected fields
        ValidateJsonPatch(patchDocument);

        var entity = await _context.FindAsync<TEntity>([id], cancellationToken);

        if (entity == null)
            return null;

        patchDocument.ApplyTo(entity);
        _context.Set<TEntity>().Attach(entity).State = EntityState.Modified;

        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <summary>
    /// Validates that the JSON Patch document doesn't try to modify protected fields.
    /// </summary>
    /// <param name="patchDocument">The patch document to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when patch tries to modify protected fields.</exception>
    protected virtual void ValidateJsonPatch(JsonPatchDocument patchDocument)
    {
        // List of protected fields that should not be modified via patch
        var protectedFields = new[] { "id", "createdby", "createddate", "activated" };

        foreach (var operation in patchDocument.Operations)
        {
            var path = operation.path?.ToLowerInvariant().TrimStart('/');

            if (string.IsNullOrEmpty(path))
                continue;

            if (protectedFields.Any(field => path.Equals(field, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"Cannot modify protected field '{operation.path}' via patch operation. " +
                    $"Protected fields: {string.Join(", ", protectedFields)}");
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        var entity = await _context.Set<TEntity>()
            .FindAsync([id], cancellationToken);

        if (entity == null)
            return false;

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.Activated = false;

            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not implement ISoftDeletable interface.");
    }

    /// <inheritdoc/>
    public virtual async Task<int> SoftDeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.ToList();
        if (!idList.Any())
            throw new ArgumentException("Ids collection cannot be empty.", nameof(ids));

        if (!typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not implement ISoftDeletable interface.");

        return await _context.Set<TEntity>()
            .Where(e => idList.Contains(EF.Property<int>(e, "Id")))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(e => EF.Property<bool>(e, "Activated"), false),
                cancellationToken
            );
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        var entity = await _context.Set<TEntity>()
            .FindAsync([id], cancellationToken);

        if (entity == null)
            return false;

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.Activated = true;

            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not implement ISoftDeletable interface.");
    }

    /// <inheritdoc/>
    public virtual async Task<int> ReactivateRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.ToList();
        if (!idList.Any())
            throw new ArgumentException("Ids collection cannot be empty.", nameof(ids));
        if (!typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not implement ISoftDeletable interface.");

        return await _context.Set<TEntity>()
            .Where(e => idList.Contains(EF.Property<int>(e, "Id")))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(e => EF.Property<bool>(e, "Activated"), true),
                cancellationToken
            );
    }
}
