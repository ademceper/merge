using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// TeamMember Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TeamMember : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = "Member"; // Member, Lead, Admin
    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true;
    
    // Navigation properties
    public Team Team { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private TeamMember() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static TeamMember Create(
        Guid teamId,
        Guid userId,
        string role = "Member")
    {
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(role, nameof(role));
        Guard.AgainstLength(role, 50, nameof(role));

        // Role validation
        var validRoles = new[] { "Member", "Lead", "Admin" };
        if (!validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            throw new DomainException($"Geçersiz rol: {role}. Geçerli roller: {string.Join(", ", validRoles)}");

        var teamMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - Add domain event (Team aggregate root'a event eklenmeli)
        // NOT: TeamMember bir aggregate root değil, bu yüzden event'i Team entity'ye eklemek gerekir
        // Ancak burada event oluşturulup Team entity'ye eklenmesi service layer'da yapılacak

        return teamMember;
    }

    // ✅ BOLUM 1.1: Domain Method - Update role
    public void UpdateRole(string role)
    {
        Guard.AgainstNullOrEmpty(role, nameof(role));
        Guard.AgainstLength(role, 50, nameof(role));

        // Role validation
        var validRoles = new[] { "Member", "Lead", "Admin" };
        if (!validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            throw new DomainException($"Geçersiz rol: {role}. Geçerli roller: {string.Join(", ", validRoles)}");

        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Activate member
    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Takım üyesi zaten aktif");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate member
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Takım üyesi zaten pasif");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Delete member (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Takım üyesi zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

