namespace BrahmCQRS.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// Gets the name of the entity type.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the identifier of the entity that was not found.
    /// </summary>
    public object EntityId { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="entityId">The identifier of the entity that was not found.</param>
    public EntityNotFoundException(string entityName, object entityId)
        : base($"Entity '{entityName}' with id '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
    public EntityNotFoundException() : base("Error 404. Entry Not Found") 
    {
        EntityName = string.Empty;
        EntityId = string.Empty;
    }
    public EntityNotFoundException(string message) : base(message) 
    {
        EntityName = string.Empty;
        EntityId = string.Empty;
    }

    /// <summary>
    /// Throws a EntityNotFoundException if the provided value is null.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">Value to validate.</param>
    /// <param name="message">Optional custom message.</param>
    public static void ThrowIfNull<T>(T? value, string? message = null)
        where T : class
    {
        if (value is null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new EntityNotFoundException();

            throw new EntityNotFoundException(message);
        }
    }

    /// <summary>
    /// Throws a EntityNotFoundException with entity information if value is null.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="value">Entity instance.</param>
    /// <param name="entityName">Name of the entity type.</param>
    /// <param name="key">Key value that was not found.</param>
    public static void ThrowIfNull<T>(T? value, string entityName, object key)
        where T : class
    {
        if (value is null)
        {
            throw new EntityNotFoundException(entityName, key);
        }
    }
}
