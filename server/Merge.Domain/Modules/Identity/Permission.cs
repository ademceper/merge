using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// Permission Entity - İzin tanımları
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// </summary>
public class Permission : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// İzin adı (örn: "products.create", "orders.view")
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// İzin açıklaması
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// İzin kategorisi (örn: "Products", "Orders", "Users")
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    
    /// <summary>
    /// Kaynak (Resource) - İzinin hangi kaynağa ait olduğu (örn: "products", "orders")
    /// </summary>
    public string Resource { get; private set; } = string.Empty;
    
    /// <summary>
    /// Aksiyon (Action) - İzinin hangi aksiyona ait olduğu (örn: "create", "view", "delete")
    /// </summary>
    public string Action { get; private set; } = string.Empty;
    
    /// <summary>
    /// Sistem izni mi? (Sistem izinleri silinemez)
    /// </summary>
    public bool IsSystemPermission { get; private set; } = false;
    
    // Navigation properties
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { }

    public static Permission Create(
        string name,
        string category,
        string resource,
        string action,
        string? description = null,
        bool isSystemPermission = false)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 200, nameof(name));
        Guard.AgainstNullOrEmpty(category, nameof(category));
        Guard.AgainstLength(category, 100, nameof(category));
        Guard.AgainstNullOrEmpty(resource, nameof(resource));
        Guard.AgainstLength(resource, 100, nameof(resource));
        Guard.AgainstNullOrEmpty(action, nameof(action));
        Guard.AgainstLength(action, 50, nameof(action));
        
        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, 500, nameof(description));
        }

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            Resource = resource,
            Action = action,
            Description = description,
            IsSystemPermission = isSystemPermission,
            CreatedAt = DateTime.UtcNow
        };

        permission.AddDomainEvent(new PermissionCreatedEvent(permission.Id, name, category, resource, action));

        return permission;
    }

    public void Update(
        string? name = null,
        string? category = null,
        string? resource = null,
        string? action = null,
        string? description = null)
    {
        if (IsSystemPermission)
            throw new DomainException("Sistem izinleri güncellenemez");

        if (IsDeleted)
            throw new DomainException("Silinmiş izin güncellenemez");

        var changed = false;

        if (name is not null && name != Name)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstLength(name, 200, nameof(name));
            Name = name;
            changed = true;
        }

        if (category is not null && category != Category)
        {
            Guard.AgainstNullOrEmpty(category, nameof(category));
            Guard.AgainstLength(category, 100, nameof(category));
            Category = category;
            changed = true;
        }

        if (resource is not null && resource != Resource)
        {
            Guard.AgainstNullOrEmpty(resource, nameof(resource));
            Guard.AgainstLength(resource, 100, nameof(resource));
            Resource = resource;
            changed = true;
        }

        if (action is not null && action != Action)
        {
            Guard.AgainstNullOrEmpty(action, nameof(action));
            Guard.AgainstLength(action, 50, nameof(action));
            Action = action;
            changed = true;
        }

        if (description is not null && description != Description)
        {
            Guard.AgainstLength(description, 500, nameof(description));
            Description = description;
            changed = true;
        }

        if (changed)
        {
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new PermissionUpdatedEvent(Id, Name, Category, Resource, Action));
        }
    }

    public void Delete()
    {
        if (IsSystemPermission)
            throw new DomainException("Sistem izinleri silinemez");

        if (IsDeleted)
            throw new DomainException("İzin zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PermissionDeletedEvent(Id, Name));
    }
}
