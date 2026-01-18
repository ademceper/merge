using Merge.Domain.SharedKernel;
using Microsoft.AspNetCore.Identity;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// Role Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// NOT: IdentityRole'dan türüyor, bu yüzden BaseEntity'den türemiyor.
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Role : IdentityRole<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public static Role Create(string name, string? description = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 256, nameof(name)); // IdentityRole.Name max length
        
        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, 500, nameof(description));
        }
        
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            CreatedAt = DateTime.UtcNow,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        role.AddDomainEvent(new RoleCreatedEvent(role.Id, name, description));

        return role;
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Remove(domainEvent);
    }

    public void UpdateDescription(string? description)
    {
        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, 500, nameof(description));
        }
        
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RoleUpdatedEvent(Id, Name ?? string.Empty, Description));
    }

    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 256, nameof(name)); // IdentityRole.Name max length
        
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RoleUpdatedEvent(Id, Name, Description));
    }
}
