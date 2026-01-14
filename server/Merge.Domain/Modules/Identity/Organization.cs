using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using AddressValueObject = Merge.Domain.ValueObjects.Address;

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
    
    // ✅ BOLUM 1.3: Value Objects - Email backing field (EF Core compatibility)
    private string? _email;
    public string? Email 
    { 
        get => _email; 
        private set => _email = value; 
    }
    
    // ✅ BOLUM 1.3: Value Objects - PhoneNumber backing field (EF Core compatibility)
    private string? _phone;
    public string? Phone 
    { 
        get => _phone; 
        private set => _phone = value; 
    }
    
    // ✅ BOLUM 1.3: Value Objects - URL backing field (EF Core compatibility)
    private string? _website;
    public string? Website 
    { 
        get => _website; 
        private set => _website = value; 
    }
    
    // ✅ BOLUM 1.3: Value Objects - Address backing fields (EF Core compatibility)
    // Address Value Object için tüm alanlar zorunlu olduğundan, nullable olarak tutuyoruz
    // Address Value Object'i oluşturmak için tüm alanların dolu olması gerekiyor
    private string? _addressLine1;
    private string? _addressLine2;
    private string? _city;
    private string? _state;
    private string? _postalCode;
    private string? _country;
    
    // Database columns (EF Core mapping) - Backward compatibility için
    // Address property'si AddressLine1'i döndürür (backward compatibility)
    public string? Address 
    { 
        get => _addressLine1; 
        private set => _addressLine1 = value; 
    }
    
    // ✅ BOLUM 1.3: Value Objects - AddressLine2 property (backing field access)
    public string? AddressLine2 
    { 
        get => _addressLine2; 
        private set => _addressLine2 = value; 
    }
    
    public string? City 
    { 
        get => _city; 
        private set => _city = value; 
    }
    
    public string? State 
    { 
        get => _state; 
        private set => _state = value; 
    }
    
    public string? PostalCode 
    { 
        get => _postalCode; 
        private set => _postalCode = value; 
    }
    
    public string? Country 
    { 
        get => _country; 
        private set => _country = value; 
    }
    
    // ✅ BOLUM 1.3: Value Object property (computed from backing fields)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public AddressValueObject? AddressValueObject
    {
        get
        {
            // Address Value Object için tüm zorunlu alanların dolu olması gerekiyor
            if (string.IsNullOrWhiteSpace(_addressLine1) || 
                string.IsNullOrWhiteSpace(_city) || 
                string.IsNullOrWhiteSpace(_postalCode) || 
                string.IsNullOrWhiteSpace(_country))
                return null;
            
            // ✅ BOLUM 1.3: Value Objects - Address Value Object oluştur (validation otomatik yapılır)
            // Eğer Address Value Object oluşturulamazsa (validation hatası), exception fırlatılır
            // Bu doğru davranıştır - invalid address data'sı için exception fırlatılmalı
            return new AddressValueObject(
                _addressLine1,
                _city,
                _country,
                _postalCode,
                _addressLine2, // ✅ AddressLine2 backing field'ı kullanılıyor
                _state);
        }
    }
    
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    public bool IsVerified { get; private set; } = false;
    public DateTime? VerifiedAt { get; private set; }
    public string? Settings { get; private set; } // JSON for organization-specific settings
    
    // ✅ BOLUM 1.1: Rich Domain Model - Encapsulated collections with backing fields
    // ✅ BOLUM 7.1.9: Collection Expressions (C# 12) - List yerine collection expression
    private readonly List<User> _users = [];
    private readonly List<Team> _teams = [];
    
    // ✅ BOLUM 1.1: Rich Domain Model - Navigation properties (read-only collections)
    // ✅ BOLUM 1.4: Aggregate Root Pattern - Users ve Teams'e sadece Organization üzerinden erişim
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();
    public IReadOnlyCollection<Team> Teams => _teams.AsReadOnly();

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
        string? addressLine2 = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? settings = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        // ✅ BOLUM 1.1: Name validation - Minimum length kontrolü (DTO ile uyumlu)
        Guard.AgainstOutOfRange(name.Length, 2, 200, nameof(name));
        
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
        
        if (!string.IsNullOrEmpty(settings))
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
            
            // ✅ BOLUM 1.1: JSON validation - Settings JSON format kontrolü
            try
            {
                System.Text.Json.JsonDocument.Parse(settings);
            }
            catch (System.Text.Json.JsonException)
            {
                throw new DomainException("Settings geçerli bir JSON formatında olmalıdır");
            }
        }

        // ✅ BOLUM 1.3: Value Objects - Email validation
        string? validatedEmail = null;
        if (!string.IsNullOrEmpty(email))
        {
            var emailValueObject = new Email(email);
            validatedEmail = emailValueObject.Value;
        }

        // ✅ BOLUM 1.3: Value Objects - PhoneNumber validation
        string? validatedPhone = null;
        if (!string.IsNullOrEmpty(phone))
        {
            var phoneValueObject = new PhoneNumber(phone);
            validatedPhone = phoneValueObject.Value;
        }

        // ✅ BOLUM 1.3: Value Objects - URL validation
        string? validatedWebsite = null;
        if (!string.IsNullOrEmpty(website))
        {
            var urlValueObject = new Url(website);
            validatedWebsite = urlValueObject.Value;
        }
        
        // ✅ BOLUM 1.3: Value Objects - Address validation
        // Address Value Object için tüm alanlar zorunlu olduğundan, sadece tüm alanlar doluysa oluşturuyoruz
        if (!string.IsNullOrEmpty(address) && 
            !string.IsNullOrEmpty(city) && 
            !string.IsNullOrEmpty(postalCode) && 
            !string.IsNullOrEmpty(country))
        {
            // ✅ BOLUM 1.3: Value Objects - Address Value Object validation (tüm alanlar zorunlu)
            // Address Value Object constructor içinde validation yapılır
            var addressValueObject = new AddressValueObject(
                address,
                city,
                country,
                postalCode,
                addressLine2, // ✅ AddressLine2 parametresi kullanılıyor
                state);
        }
        else
        {
            // Kısmi adres bilgileri için ayrı ayrı validation
            if (!string.IsNullOrEmpty(address))
                Guard.AgainstLength(address, 500, nameof(address));
            if (!string.IsNullOrEmpty(addressLine2))
                Guard.AgainstLength(addressLine2, 500, nameof(addressLine2));
            if (!string.IsNullOrEmpty(city))
                Guard.AgainstLength(city, 100, nameof(city));
            if (!string.IsNullOrEmpty(state))
                Guard.AgainstLength(state, 100, nameof(state));
            if (!string.IsNullOrEmpty(postalCode))
                Guard.AgainstLength(postalCode, 20, nameof(postalCode));
            if (!string.IsNullOrEmpty(country))
                Guard.AgainstLength(country, 100, nameof(country));
        }

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            LegalName = legalName,
            TaxNumber = taxNumber,
            RegistrationNumber = registrationNumber,
            Email = validatedEmail,
            Phone = validatedPhone,
            Website = validatedWebsite,
            Address = address,
            AddressLine2 = addressLine2, // ✅ AddressLine2 backing field'a set ediliyor
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
        string? addressLine2 = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? settings = null)
    {
        // ✅ BOLUM 1.1: Business Invariants - Silinmiş organizasyon güncellenemez
        if (IsDeleted)
            throw new DomainException("Silinmiş organizasyon güncellenemez");

        var changedFields = new List<string>();

        // ✅ BOLUM 1.1: Name validation - null check, empty check ve minimum length kontrolü (DTO ile uyumlu)
        if (name != null && name != Name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Organizasyon adı boş olamaz");
            
            // ✅ BOLUM 1.1: Name validation - Minimum length kontrolü (Create ile uyumlu)
            Guard.AgainstOutOfRange(name.Length, 2, 200, nameof(name));
            Name = name;
            changedFields.Add(nameof(Name));
        }

        if (email != null && email != Email)
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
            changedFields.Add(nameof(Email));
        }

        if (phone != null && phone != Phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                Phone = null;
            }
            else
            {
                // ✅ BOLUM 1.3: Value Objects - PhoneNumber validation
                var phoneValueObject = new PhoneNumber(phone);
                Phone = phoneValueObject.Value;
            }
            changedFields.Add(nameof(Phone));
        }

        if (website != null && website != Website)
        {
            if (string.IsNullOrEmpty(website))
            {
                Website = null;
            }
            else
            {
                // ✅ BOLUM 1.3: Value Objects - URL validation
                var urlValueObject = new Url(website);
                Website = urlValueObject.Value;
            }
            changedFields.Add(nameof(Website));
        }

        if (legalName != null && legalName != LegalName)
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
            changedFields.Add(nameof(LegalName));
        }
        
        if (taxNumber != null && taxNumber != TaxNumber)
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
            changedFields.Add(nameof(TaxNumber));
        }
        
        if (registrationNumber != null && registrationNumber != RegistrationNumber)
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
            changedFields.Add(nameof(RegistrationNumber));
        }
        
        // ✅ BOLUM 1.3: Value Objects - Address validation
        // Address Value Object için tüm alanlar zorunlu olduğundan, sadece tüm alanlar doluysa oluşturuyoruz
        var addressChanged = (address != null && address != Address) ||
                             (addressLine2 != null && addressLine2 != AddressLine2) ||
                             (city != null && city != City) ||
                             (state != null && state != State) ||
                             (postalCode != null && postalCode != PostalCode) ||
                             (country != null && country != Country);
        
        if (addressChanged)
        {
            // Tüm adres alanları doluysa Address Value Object validation
            var finalAddress = address ?? Address;
            var finalAddressLine2 = addressLine2 ?? AddressLine2;
            var finalCity = city ?? City;
            var finalState = state ?? State;
            var finalPostalCode = postalCode ?? PostalCode;
            var finalCountry = country ?? Country;
            
            if (!string.IsNullOrEmpty(finalAddress) && 
                !string.IsNullOrEmpty(finalCity) && 
                !string.IsNullOrEmpty(finalPostalCode) && 
                !string.IsNullOrEmpty(finalCountry))
            {
                // ✅ BOLUM 1.3: Value Objects - Address Value Object validation (tüm alanlar zorunlu)
                // Address Value Object constructor içinde validation yapılır
                var addressValueObject = new AddressValueObject(
                    finalAddress,
                    finalCity,
                    finalCountry,
                    finalPostalCode,
                    finalAddressLine2, // ✅ AddressLine2 parametresi kullanılıyor
                    finalState);
            }
            
            // Kısmi adres bilgileri için ayrı ayrı validation ve update
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
                changedFields.Add(nameof(Address));
            }
            
            if (addressLine2 != null)
            {
                if (string.IsNullOrEmpty(addressLine2))
                {
                    AddressLine2 = null;
                }
                else
                {
                    Guard.AgainstLength(addressLine2, 500, nameof(addressLine2));
                    AddressLine2 = addressLine2;
                }
                changedFields.Add(nameof(AddressLine2));
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
                changedFields.Add(nameof(City));
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
                changedFields.Add(nameof(State));
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
                changedFields.Add(nameof(PostalCode));
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
                changedFields.Add(nameof(Country));
            }
        }
        
        if (settings != null && settings != Settings)
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
            
            // ✅ BOLUM 1.1: JSON validation - Settings JSON format kontrolü
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

            // ✅ BOLUM 1.5: Domain Events - Add domain event with changed fields
            AddDomainEvent(new OrganizationUpdatedEvent(Id, Name, changedFields));
        }
    }

    // ✅ BOLUM 1.1: Domain Method - Verify organization
    public void Verify()
    {
        // ✅ BOLUM 1.1: Business Invariants - Silinmiş organizasyon doğrulanamaz
        if (IsDeleted)
            throw new DomainException("Silinmiş organizasyon doğrulanamaz");

        // ✅ BOLUM 1.1: Business Invariants - Askıya alınmış organizasyon doğrulanamaz
        if (Status == EntityStatus.Suspended)
            throw new DomainException("Askıya alınmış organizasyon doğrulanamaz");

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

        if (Status == EntityStatus.Deleted)
            throw new DomainException("Silinmiş organizasyon aktif edilemez");

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

