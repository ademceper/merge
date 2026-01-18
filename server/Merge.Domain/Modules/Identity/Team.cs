using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// Team Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Team : BaseEntity, IAggregateRoot
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? TeamLeadId { get; private set; } // Team leader user ID
    public bool IsActive { get; private set; } = true;
    public string? Settings { get; private set; } // JSON for team-specific settings
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;
    public User? TeamLead { get; private set; }
    
    private readonly List<TeamMember> _members = [];
    
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı (User entity'sinde de aynı pattern kullanılıyor)
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    private Team() { }

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
        
        if (!string.IsNullOrEmpty(settings))
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
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

        team.AddDomainEvent(new TeamCreatedEvent(team.Id, organizationId, team.Name));

        return team;
    }

    public void Update(
        string? name = null,
        string? description = null,
        Guid? teamLeadId = null,
        string? settings = null)
    {
        if (IsDeleted)
            throw new DomainException("Silinmiş takım güncellenemez");

        List<string> changedFields = [];

        if (name != null && name != Name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Takım adı boş olamaz");
            
            Guard.AgainstOutOfRange(name.Length, 1, 200, nameof(name));
            Name = name;
            changedFields.Add(nameof(Name));
        }

        if (description != null && description != Description)
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
            changedFields.Add(nameof(Description));
        }

        if (teamLeadId.HasValue && teamLeadId.Value != TeamLeadId)
        {
            Guard.AgainstDefault(teamLeadId.Value, nameof(teamLeadId));
            TeamLeadId = teamLeadId;
            changedFields.Add(nameof(TeamLeadId));
        }
        else if (teamLeadId == null && TeamLeadId != null)
        {
            TeamLeadId = null;
            changedFields.Add(nameof(TeamLeadId));
        }

        if (settings != null && settings != Settings)
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
            
            if (!string.IsNullOrEmpty(settings))
            {
                try
                {
                    System.Text.Json.JsonDocument.Parse(settings);
                }
                catch (System.Text.Json.JsonException)
                {
                    throw new DomainException("Settings geçerli bir JSON formatında olmalıdır");
                }
            }
            
            Settings = settings;
            changedFields.Add(nameof(Settings));
        }

        // Sadece değişiklik varsa UpdatedAt ve event ekle
        if (changedFields.Count > 0)
        {
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new TeamUpdatedEvent(Id, OrganizationId, Name));
        }
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Takım zaten aktif");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TeamActivatedEvent(Id, OrganizationId, Name));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Takım zaten pasif");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TeamDeactivatedEvent(Id, OrganizationId, Name));
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Takım zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TeamDeletedEvent(Id, OrganizationId, Name));
    }
}

