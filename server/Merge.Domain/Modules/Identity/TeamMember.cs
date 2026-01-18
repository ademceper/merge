using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Enums;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// TeamMember Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string Role YASAK)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TeamMember : BaseEntity
{
    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    
    public TeamMemberRole Role { get; private set; } = TeamMemberRole.Member;
    
    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true;
    
    // Navigation properties
    public Team Team { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private TeamMember() { }

    public static TeamMember Create(
        Guid teamId,
        Guid userId,
        TeamMemberRole role = TeamMemberRole.Member)
    {
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(userId, nameof(userId));

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

        // NOT: TeamMember bir aggregate root değil, bu yüzden event'i Team entity'ye eklemek gerekir
        // Ancak burada event oluşturulup Team entity'ye eklenmesi service layer'da yapılacak

        return teamMember;
    }

    public void UpdateRole(TeamMemberRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Takım üyesi zaten aktif");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Takım üyesi zaten pasif");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Takım üyesi zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

