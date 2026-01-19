using Merge.Domain.SharedKernel;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// RolePermission Entity - Rol-İzin ilişkisi (Many-to-Many)
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    
    // Navigation properties
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { }

    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        Guard.AgainstDefault(roleId, nameof(roleId));
        Guard.AgainstDefault(permissionId, nameof(permissionId));

        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
