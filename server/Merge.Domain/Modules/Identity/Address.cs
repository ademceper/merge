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
    public ICollection<Order> Orders { get; private set; } = [];

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    private Address() { }

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

        address.AddDomainEvent(new AddressCreatedEvent(address.Id, userId, city, country, isDefault));

        return address;
    }

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

        AddDomainEvent(new AddressUpdatedEvent(Id, UserId));
    }

    public void SetAsDefault()
    {
        if (IsDefault) return;
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AddressSetAsDefaultEvent(Id, UserId));
    }

    public void RemoveDefault()
    {
        if (!IsDefault) return;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AddressRemovedDefaultEvent(Id, UserId));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Address is already deleted");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AddressDeletedEvent(Id, UserId));
    }
}

