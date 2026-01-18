using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// ShippingAddress Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU - Optional, navigation property için gerekli değil)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ShippingAddress : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty; // Home, Work, Other, etc.
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; } = false;
    public bool IsActive { get; private set; } = true;
    public string? Instructions { get; private set; } // Delivery instructions
    
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<Order> Orders { get; private set; } = [];

    private ShippingAddress() { }

    public static ShippingAddress Create(
        Guid userId,
        string label,
        string firstName,
        string lastName,
        string phone,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country,
        bool isDefault = false,
        string? instructions = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(label, nameof(label));
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
        Guard.AgainstNullOrEmpty(phone, nameof(phone));
        Guard.AgainstNullOrEmpty(addressLine1, nameof(addressLine1));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(country, nameof(country));

        var address = new ShippingAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Label = label,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = country,
            IsDefault = isDefault,
            IsActive = true,
            Instructions = instructions,
            CreatedAt = DateTime.UtcNow
        };

        address.AddDomainEvent(new ShippingAddressCreatedEvent(
            address.Id,
            address.UserId,
            address.Label,
            address.City,
            address.Country,
            address.IsDefault));

        return address;
    }

    public void UpdateDetails(
        string label,
        string firstName,
        string lastName,
        string phone,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country,
        string? instructions = null)
    {
        Guard.AgainstNullOrEmpty(label, nameof(label));
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
        Guard.AgainstNullOrEmpty(phone, nameof(phone));
        Guard.AgainstNullOrEmpty(addressLine1, nameof(addressLine1));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(country, nameof(country));

        Label = label;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        Instructions = instructions;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingAddressUpdatedEvent(Id, UserId));
    }

    public void SetAsDefault()
    {
        if (IsDefault) return;

        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingAddressSetAsDefaultEvent(Id, UserId));
    }

    public void UnsetAsDefault()
    {
        if (!IsDefault) return;

        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingAddressUnsetAsDefaultEvent(Id, UserId));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingAddressActivatedEvent(Id, UserId));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingAddressDeactivatedEvent(Id, UserId));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ShippingAddressDeletedEvent(Id, UserId));
    }
}

