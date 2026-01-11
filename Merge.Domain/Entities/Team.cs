using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// Team Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Team : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? TeamLeadId { get; private set; } // Team leader user ID
    public bool IsActive { get; private set; } = true;
    public string? Settings { get; private set; } // JSON for team-specific settings
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;
    public User? TeamLead { get; private set; }
    public ICollection<TeamMember> Members { get; private set; } = new List<TeamMember>();

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı (User entity'sinde de aynı pattern kullanılıyor)
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Team() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Team Create(
        Guid organizationId,
        string name,
        string? description = null,
        Guid? teamLeadId = null,
        string? settings = null)
    {
        Guard.AgainstDefault(organizationId, nameof(organizationId));
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 200, nameof(name));

        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, 1000, nameof(description));
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name,
            Description = description,
            TeamLeadId = teamLeadId,
            IsActive = true,
            Settings = settings,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        team.AddDomainEvent(new TeamCreatedEvent(team.Id, organizationId, team.Name));

        return team;
    }

    // ✅ BOLUM 1.1: Domain Method - Update team
    public void Update(
        string? name = null,
        string? description = null,
        Guid? teamLeadId = null,
        string? settings = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstLength(name, 200, nameof(name));
            Name = name;
        }

        if (description != null)
        {
            if (string.IsNullOrEmpty(description))
            {
                Description = null;
            }
            else
            {
                Guard.AgainstLength(description, 1000, nameof(description));
                Description = description;
            }
        }

        if (teamLeadId.HasValue)
        {
            Guard.AgainstDefault(teamLeadId.Value, nameof(teamLeadId));
            TeamLeadId = teamLeadId;
        }
        else if (teamLeadId == null)
        {
            TeamLeadId = null;
        }

        if (settings != null) Settings = settings;

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new TeamUpdatedEvent(Id, OrganizationId, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate team
    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Takım zaten aktif");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new TeamActivatedEvent(Id, OrganizationId, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate team
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Takım zaten pasif");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new TeamDeactivatedEvent(Id, OrganizationId, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Delete team (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Takım zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new TeamDeletedEvent(Id, OrganizationId, Name));
    }
}

