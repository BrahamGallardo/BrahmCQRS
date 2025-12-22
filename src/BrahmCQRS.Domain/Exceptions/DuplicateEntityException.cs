namespace BrahmCQRS.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a duplicate entity.
/// </summary>
public class DuplicateEntityException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEntityException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="entityId">The identifier of the duplicate entity.</param>
    public DuplicateEntityException(string entityName, object entityId)
        : base($"Entity '{entityName}' with id '{entityId}' already exists.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// Gets the name of the entity type.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the identifier of the duplicate entity.
    /// </summary>
    public object EntityId { get; }
}
