using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// Organization Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Organization : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? TaxNumber { get; private set; } // Tax ID / VAT Number
    public string? RegistrationNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    public bool IsVerified { get; private set; } = false;
    public DateTime? VerifiedAt { get; private set; }
    public string? Settings { get; private set; } // JSON for organization-specific settings
    
    // Navigation properties
    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<Team> Teams { get; private set; } = new List<Team>();

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

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation - Remove domain event
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Organization() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Organization Create(
        string name,
        string? legalName = null,
        string? taxNumber = null,
        string? registrationNumber = null,
        string? email = null,
        string? phone = null,
        string? website = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? settings = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 200, nameof(name));
        
        if (!string.IsNullOrEmpty(legalName))
        {
            Guard.AgainstLength(legalName, 200, nameof(legalName));
        }
        
        if (!string.IsNullOrEmpty(taxNumber))
        {
            Guard.AgainstLength(taxNumber, 50, nameof(taxNumber));
        }
        
        if (!string.IsNullOrEmpty(registrationNumber))
        {
            Guard.AgainstLength(registrationNumber, 50, nameof(registrationNumber));
        }
        
        if (!string.IsNullOrEmpty(phone))
        {
            Guard.AgainstLength(phone, 50, nameof(phone));
        }
        
        if (!string.IsNullOrEmpty(address))
        {
            Guard.AgainstLength(address, 500, nameof(address));
        }
        
        if (!string.IsNullOrEmpty(city))
        {
            Guard.AgainstLength(city, 100, nameof(city));
        }
        
        if (!string.IsNullOrEmpty(state))
        {
            Guard.AgainstLength(state, 100, nameof(state));
        }
        
        if (!string.IsNullOrEmpty(postalCode))
        {
            Guard.AgainstLength(postalCode, 20, nameof(postalCode));
        }
        
        if (!string.IsNullOrEmpty(country))
        {
            Guard.AgainstLength(country, 100, nameof(country));
        }
        
        if (!string.IsNullOrEmpty(settings))
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
        }

        if (!string.IsNullOrEmpty(email))
        {
            // ✅ BOLUM 1.3: Value Objects - Email validation
            var emailValueObject = new Email(email);
            email = emailValueObject.Value;
        }

        if (!string.IsNullOrEmpty(website))
        {
            // URL validation
            if (!Uri.TryCreate(website, UriKind.Absolute, out _))
                throw new DomainException("Geçersiz website URL formatı");
        }

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            LegalName = legalName,
            TaxNumber = taxNumber,
            RegistrationNumber = registrationNumber,
            Email = email,
            Phone = phone,
            Website = website,
            Address = address,
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = country,
            Status = EntityStatus.Active,
            IsVerified = false,
            Settings = settings,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        organization.AddDomainEvent(new OrganizationCreatedEvent(organization.Id, organization.Name, organization.Email));

        return organization;
    }

    // ✅ BOLUM 1.1: Domain Method - Update organization
    public void Update(
        string? name = null,
        string? legalName = null,
        string? taxNumber = null,
        string? registrationNumber = null,
        string? email = null,
        string? phone = null,
        string? website = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? settings = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstLength(name, 200, nameof(name));
            Name = name;
        }

        if (email != null)
        {
            if (string.IsNullOrEmpty(email))
            {
                Email = null;
            }
            else
            {
                // ✅ BOLUM 1.3: Value Objects - Email validation
                var emailValueObject = new Email(email);
                Email = emailValueObject.Value;
            }
        }

        if (website != null)
        {
            if (string.IsNullOrEmpty(website))
            {
                Website = null;
            }
            else
            {
                if (!Uri.TryCreate(website, UriKind.Absolute, out _))
                    throw new DomainException("Geçersiz website URL formatı");
                Website = website;
            }
        }

        if (legalName != null)
        {
            if (string.IsNullOrEmpty(legalName))
            {
                LegalName = null;
            }
            else
            {
                Guard.AgainstLength(legalName, 200, nameof(legalName));
                LegalName = legalName;
            }
        }
        
        if (taxNumber != null)
        {
            if (string.IsNullOrEmpty(taxNumber))
            {
                TaxNumber = null;
            }
            else
            {
                Guard.AgainstLength(taxNumber, 50, nameof(taxNumber));
                TaxNumber = taxNumber;
            }
        }
        
        if (registrationNumber != null)
        {
            if (string.IsNullOrEmpty(registrationNumber))
            {
                RegistrationNumber = null;
            }
            else
            {
                Guard.AgainstLength(registrationNumber, 50, nameof(registrationNumber));
                RegistrationNumber = registrationNumber;
            }
        }
        
        if (phone != null)
        {
            if (string.IsNullOrEmpty(phone))
            {
                Phone = null;
            }
            else
            {
                Guard.AgainstLength(phone, 50, nameof(phone));
                Phone = phone;
            }
        }
        
        if (address != null)
        {
            if (string.IsNullOrEmpty(address))
            {
                Address = null;
            }
            else
            {
                Guard.AgainstLength(address, 500, nameof(address));
                Address = address;
            }
        }
        
        if (city != null)
        {
            if (string.IsNullOrEmpty(city))
            {
                City = null;
            }
            else
            {
                Guard.AgainstLength(city, 100, nameof(city));
                City = city;
            }
        }
        
        if (state != null)
        {
            if (string.IsNullOrEmpty(state))
            {
                State = null;
            }
            else
            {
                Guard.AgainstLength(state, 100, nameof(state));
                State = state;
            }
        }
        
        if (postalCode != null)
        {
            if (string.IsNullOrEmpty(postalCode))
            {
                PostalCode = null;
            }
            else
            {
                Guard.AgainstLength(postalCode, 20, nameof(postalCode));
                PostalCode = postalCode;
            }
        }
        
        if (country != null)
        {
            if (string.IsNullOrEmpty(country))
            {
                Country = null;
            }
            else
            {
                Guard.AgainstLength(country, 100, nameof(country));
                Country = country;
            }
        }
        
        if (settings != null)
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
            Settings = settings;
        }

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OrganizationUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Verify organization
    public void Verify()
    {
        if (IsVerified)
            throw new DomainException("Organizasyon zaten doğrulanmış");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OrganizationVerifiedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Suspend organization
    public void Suspend()
    {
        if (Status == EntityStatus.Suspended)
            throw new DomainException("Organizasyon zaten askıya alınmış");

        if (Status == EntityStatus.Deleted)
            throw new DomainException("Silinmiş organizasyon askıya alınamaz");

        Status = EntityStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OrganizationSuspendedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate organization
    public void Activate()
    {
        if (Status == EntityStatus.Active)
            throw new DomainException("Organizasyon zaten aktif");

        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OrganizationActivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Delete organization (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Organizasyon zaten silinmiş");

        IsDeleted = true;
        Status = EntityStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OrganizationDeletedEvent(Id, Name));
    }
}

