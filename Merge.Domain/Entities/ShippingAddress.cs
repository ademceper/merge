using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// ShippingAddress Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ShippingAddress : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
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
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ShippingAddress() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        // ✅ BOLUM 1.5: Domain Events - ShippingAddressCreatedEvent
        address.AddDomainEvent(new ShippingAddressCreatedEvent(
            address.Id,
            address.UserId,
            address.Label,
            address.City,
            address.Country,
            address.IsDefault));

        return address;
    }

    // ✅ BOLUM 1.1: Domain Method - Update address details
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

        // ✅ BOLUM 1.5: Domain Events - ShippingAddressUpdatedEvent
        AddDomainEvent(new ShippingAddressUpdatedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Set as default
    public void SetAsDefault()
    {
        if (IsDefault) return;

        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ShippingAddressSetAsDefaultEvent
        AddDomainEvent(new ShippingAddressSetAsDefaultEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Unset as default
    public void UnsetAsDefault()
    {
        if (!IsDefault) return;

        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Activate address
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate address
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

