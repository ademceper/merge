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

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation (mümkün olduğunca)
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // ✅ BOLUM 1.1: Factory Method
    public static Role Create(string name, string? description = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            CreatedAt = DateTime.UtcNow,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
         _domainEvents.Remove(domainEvent);
    }

    // ✅ BOLUM 1.1: Domain Method - Update description
    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - RoleUpdatedEvent
        AddDomainEvent(new RoleUpdatedEvent(Id, Name ?? string.Empty, Description));
    }

    // ✅ BOLUM 1.1: Domain Method - Update name
    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        
        Name = name;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - RoleUpdatedEvent
        AddDomainEvent(new RoleUpdatedEvent(Id, Name, Description));
    }
}
