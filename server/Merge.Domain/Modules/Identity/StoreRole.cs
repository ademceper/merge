using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// StoreRole Entity - Store bazlı rol atamaları
/// Bir kullanıcı farklı store'larda farklı roller alabilir
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class StoreRole : BaseEntity, IAggregateRoot
{
    public Guid StoreId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    
    /// <summary>
    /// Rol atama tarihi
    /// </summary>
    public DateTime AssignedAt { get; private set; }
    
    /// <summary>
    /// Rolü atayan kullanıcı ID
    /// </summary>
    public Guid? AssignedByUserId { get; private set; }
    
    // Navigation properties
    public Store Store { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
    public User? AssignedBy { get; private set; }

    private StoreRole() { }

    public static StoreRole Create(
        Guid storeId,
        Guid userId,
        Guid roleId,
        Guid? assignedByUserId = null)
    {
        Guard.AgainstDefault(storeId, nameof(storeId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(roleId, nameof(roleId));

        var storeRole = new StoreRole
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = assignedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        storeRole.AddDomainEvent(new StoreRoleAssignedEvent(storeRole.Id, storeId, userId, roleId));

        return storeRole;
    }

    public void Remove()
    {
        if (IsDeleted)
            throw new DomainException("Store rolü zaten kaldırılmış");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StoreRoleRemovedEvent(Id, StoreId, UserId, RoleId));
    }
}
