using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// Address Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Address : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty; // Ev, İş, vb.
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = "Türkiye";
    public bool IsDefault { get; private set; } = false;
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation - Remove domain event
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Address() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Address Create(
        Guid userId,
        string title,
        string firstName,
        string lastName,
        string phoneNumber,
        string addressLine1,
        string city,
        string district,
        string postalCode,
        string country = "Türkiye",
        string? addressLine2 = null,
        bool isDefault = false)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstLength(title, 50, nameof(title));
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstLength(firstName, 100, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
        Guard.AgainstLength(lastName, 100, nameof(lastName));
        Guard.AgainstNullOrEmpty(phoneNumber, nameof(phoneNumber));
        Guard.AgainstNullOrEmpty(addressLine1, nameof(addressLine1));
        Guard.AgainstLength(addressLine1, 200, nameof(addressLine1));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstLength(city, 100, nameof(city));
        Guard.AgainstNullOrEmpty(district, nameof(district));
        Guard.AgainstLength(district, 100, nameof(district));
        Guard.AgainstNullOrEmpty(postalCode, nameof(postalCode));
        Guard.AgainstLength(postalCode, 20, nameof(postalCode));
        Guard.AgainstNullOrEmpty(country, nameof(country));
        Guard.AgainstLength(country, 100, nameof(country));
        
        if (!string.IsNullOrEmpty(addressLine2))
        {
            Guard.AgainstLength(addressLine2, 200, nameof(addressLine2));
        }

        // ✅ BOLUM 1.3: Value Objects - PhoneNumber validation (basit format kontrolü)
        // NOT: PhoneNumber value object kullanmak için entity'yi büyük ölçüde değiştirmek gerekir
        // Şimdilik basit validation ekliyoruz
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var cleanedPhone = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (cleanedPhone.Length < 10 || cleanedPhone.Length > 15)
                throw new DomainException("Geçersiz telefon numarası formatı");
        }

        var address = new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            District = district,
            PostalCode = postalCode,
            Country = country,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        address.AddDomainEvent(new AddressCreatedEvent(address.Id, userId, city, country, isDefault));

        return address;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update address
    public void UpdateAddress(
        string title,
        string firstName,
        string lastName,
        string phoneNumber,
        string addressLine1,
        string city,
        string district,
        string postalCode,
        string? addressLine2 = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstLength(title, 50, nameof(title));
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstLength(firstName, 100, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
        Guard.AgainstLength(lastName, 100, nameof(lastName));
        Guard.AgainstNullOrEmpty(phoneNumber, nameof(phoneNumber));
        Guard.AgainstNullOrEmpty(addressLine1, nameof(addressLine1));
        Guard.AgainstLength(addressLine1, 200, nameof(addressLine1));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstLength(city, 100, nameof(city));
        Guard.AgainstNullOrEmpty(district, nameof(district));
        Guard.AgainstLength(district, 100, nameof(district));
        Guard.AgainstNullOrEmpty(postalCode, nameof(postalCode));
        Guard.AgainstLength(postalCode, 20, nameof(postalCode));
        
        if (!string.IsNullOrEmpty(addressLine2))
        {
            Guard.AgainstLength(addressLine2, 200, nameof(addressLine2));
        }

        // ✅ BOLUM 1.3: Value Objects - PhoneNumber validation
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var cleanedPhone = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (cleanedPhone.Length < 10 || cleanedPhone.Length > 15)
                throw new DomainException("Geçersiz telefon numarası formatı");
        }

        Title = title;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        District = district;
        PostalCode = postalCode;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new AddressUpdatedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set as default
    public void SetAsDefault()
    {
        if (IsDefault) return;
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new AddressSetAsDefaultEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Remove default flag
    public void RemoveDefault()
    {
        if (!IsDefault) return;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new AddressRemovedDefaultEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Address is already deleted");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new AddressDeletedEvent(Id, UserId));
    }
}

