using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// OrganizationRole Entity - Organizasyon bazlı rol atamaları
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class OrganizationRole : BaseEntity, IAggregateRoot
{
    public Guid OrganizationId { get; private set; }
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
    public Organization Organization { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
    public User? AssignedBy { get; private set; }

    private OrganizationRole() { }

    public static OrganizationRole Create(
        Guid organizationId,
        Guid userId,
        Guid roleId,
        Guid? assignedByUserId = null)
    {
        Guard.AgainstDefault(organizationId, nameof(organizationId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(roleId, nameof(roleId));

        var organizationRole = new OrganizationRole
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = assignedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        organizationRole.AddDomainEvent(new OrganizationRoleAssignedEvent(organizationRole.Id, organizationId, userId, roleId));

        return organizationRole;
    }

    public void Remove()
    {
        if (IsDeleted)
            throw new DomainException("Organizasyon rolü zaten kaldırılmış");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrganizationRoleRemovedEvent(Id, OrganizationId, UserId, RoleId));
    }
}
