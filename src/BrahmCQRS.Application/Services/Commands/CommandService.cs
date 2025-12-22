using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Domain.Contracts.Repositories;
using Microsoft.AspNetCore.JsonPatch;

namespace BrahmCQRS.Application.Services.Commands;

/// <summary>
/// Generic implementation of command service for CQRS pattern.
/// Provides operations for creating, updating, and deleting entities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandService{TEntity}"/> class.
/// </remarks>
/// <param name="repository">The command repository.</param>
public class CommandService<TEntity>(ICommandRepository<TEntity> repository) : ICommandService<TEntity> where TEntity : class
{
    private readonly ICommandRepository<TEntity> _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <inheritdoc/>
    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return await _repository.CreateAsync(entity, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return await _repository.CreateRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return await _repository.UpdateAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return await _repository.UpdateRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> PatchAsync(int id, JsonPatchDocument patchDocument, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        ArgumentNullException.ThrowIfNull(patchDocument);

        return await _repository.UpdatePatchAsync(id, patchDocument, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        return await _repository.SoftDeleteAsync(id, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<int> SoftDeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.ToList();
        if (!idList.Any())
            throw new ArgumentException("Ids collection cannot be empty.", nameof(ids));

        return await _repository.SoftDeleteRangeAsync(idList, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));

        return await _repository.ReactivateAsync(id, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<int> ReactivateRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.ToList();
        if (!idList.Any())
            throw new ArgumentException("Ids collection cannot be empty.", nameof(ids));

        return await _repository.ReactivateRangeAsync(idList, cancellationToken);
    }
}
