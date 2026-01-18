namespace Merge.Application.Exceptions;

/// <summary>
/// Resource bulunamadı exception'ı.
/// HTTP Status: 404 Not Found
/// </summary>
public class NotFoundException : MergeException
{
    /// <summary>
    /// Resource tipi (örn: "Product", "Order")
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Resource ID
    /// </summary>
    public object ResourceId { get; }

    public NotFoundException(string message)
        : base("RESOURCE_NOT_FOUND", message, 404)
    {
        ResourceType = string.Empty;
        ResourceId = Guid.Empty;
    }

    public NotFoundException(string entityName, Guid id)
        : base("RESOURCE_NOT_FOUND", $"{entityName} bulunamadı. Id: {id}", 404)
    {
        ResourceType = entityName;
        ResourceId = id;

        WithMetadata("resourceType", entityName);
        WithMetadata("resourceId", id);
    }

    public NotFoundException(string entityName, object id)
        : base("RESOURCE_NOT_FOUND", $"{entityName} bulunamadı. Id: {id}", 404)
    {
        ResourceType = entityName;
        ResourceId = id;

        WithMetadata("resourceType", entityName);
        WithMetadata("resourceId", id);
    }

    /// <summary>
    /// Generic factory method.
    /// </summary>
    public static NotFoundException For<T>(Guid id) where T : class
        => new(typeof(T).Name, id);

    /// <summary>
    /// Generic factory method.
    /// </summary>
    public static NotFoundException For<T>(string identifier) where T : class
        => new(typeof(T).Name, identifier);
}

