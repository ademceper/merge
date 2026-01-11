using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Entities;

/// <summary>
/// Store Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class Store : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid SellerId { get; private set; }
    public string StoreName { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty; // URL-friendly store name
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    // ✅ BOLUM 1.3: Value Objects - Address value object kullanımı (optional)
    // Address bilgileri string olarak tutuluyor (optional field olduğu için)
    // Eğer Address dolu ise Address value object ile validate edilir
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    public bool IsPrimary { get; private set; } = false; // Primary store for seller
    public bool IsVerified { get; private set; } = false;
    public DateTime? VerifiedAt { get; private set; }
    public string? Settings { get; private set; } // JSON for store-specific settings
    
    // Navigation properties
    public User Seller { get; private set; } = null!;
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Store() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Store Create(
        Guid sellerId,
        string storeName,
        string? description = null,
        string? logoUrl = null,
        string? bannerUrl = null,
        string? contactEmail = null,
        string? contactPhone = null,
        string? address = null,
        string? city = null,
        string? country = null,
        string? postalCode = null,
        string? settings = null)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstNullOrEmpty(storeName, nameof(storeName));

        // ✅ BOLUM 1.3: Value Objects - Validation using Value Objects (optional fields)
        string? validatedEmail = null;
        string? validatedPhone = null;
        
        if (!string.IsNullOrWhiteSpace(contactEmail))
        {
            var emailValueObject = new Email(contactEmail);
            validatedEmail = emailValueObject.Value;
        }
        
        if (!string.IsNullOrWhiteSpace(contactPhone))
        {
            var phoneValueObject = new PhoneNumber(contactPhone);
            validatedPhone = phoneValueObject.Value;
        }

        // ✅ BOLUM 1.3: Value Objects - Address validation (optional fields)
        // Eğer tüm address bilgileri verilmişse Address value object ile validate et
        string? validatedAddress = address;
        string? validatedCity = city;
        string? validatedCountry = country;
        
        if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(city) && 
            !string.IsNullOrWhiteSpace(country) && !string.IsNullOrWhiteSpace(postalCode))
        {
            var addressValueObject = new Merge.Domain.ValueObjects.Address(
                address, city, country, postalCode);
            validatedAddress = addressValueObject.AddressLine1;
            validatedCity = addressValueObject.City;
            validatedCountry = addressValueObject.Country;
        }

        var slug = GenerateSlug(storeName);

        var store = new Store
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            StoreName = storeName,
            Slug = slug,
            Description = description,
            LogoUrl = logoUrl,
            BannerUrl = bannerUrl,
            ContactEmail = validatedEmail,
            ContactPhone = validatedPhone,
            Address = validatedAddress,
            City = validatedCity,
            Country = validatedCountry,
            PostalCode = postalCode,
            Settings = settings,
            Status = EntityStatus.Active,
            IsPrimary = false,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Event - Store Created
        store.AddDomainEvent(new StoreCreatedEvent(store.Id, sellerId, storeName, slug));

        return store;
    }

    // ✅ BOLUM 1.1: Domain Method - Update store details
    public void UpdateDetails(
        string? storeName = null,
        string? description = null,
        string? logoUrl = null,
        string? bannerUrl = null,
        string? contactEmail = null,
        string? contactPhone = null,
        string? address = null,
        string? city = null,
        string? country = null,
        string? postalCode = null,
        string? settings = null)
    {
        if (Status == EntityStatus.Deleted)
            throw new DomainException("Silinmiş mağaza güncellenemez");

        if (storeName != null)
        {
            Guard.AgainstNullOrEmpty(storeName, nameof(storeName));
            StoreName = storeName;
            Slug = GenerateSlug(storeName);
        }

        if (description != null)
            Description = description;

        if (logoUrl != null)
            LogoUrl = logoUrl;

        if (bannerUrl != null)
            BannerUrl = bannerUrl;

        // ✅ BOLUM 1.3: Value Objects - Validation using Value Objects (optional fields)
        if (contactEmail != null)
        {
            var emailValueObject = new Email(contactEmail);
            ContactEmail = emailValueObject.Value;
        }

        if (contactPhone != null)
        {
            var phoneValueObject = new PhoneNumber(contactPhone);
            ContactPhone = phoneValueObject.Value;
        }

        // ✅ BOLUM 1.3: Value Objects - Address validation (optional fields)
        // Eğer tüm address bilgileri verilmişse Address value object ile validate et
        if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(city) && 
            !string.IsNullOrWhiteSpace(country) && !string.IsNullOrWhiteSpace(postalCode))
        {
            var addressValueObject = new Merge.Domain.ValueObjects.Address(
                address, city, country, postalCode);
            Address = addressValueObject.AddressLine1;
            City = addressValueObject.City;
            Country = addressValueObject.Country;
            PostalCode = addressValueObject.PostalCode;
        }
        else
        {
            // Partial address bilgileri - sadece string olarak kaydet
            if (address != null)
                Address = address;
            if (city != null)
                City = city;
            if (country != null)
                Country = country;
            if (postalCode != null)
                PostalCode = postalCode;
        }

        if (settings != null)
            Settings = settings;

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Store Updated
        AddDomainEvent(new StoreUpdatedEvent(Id, SellerId));
    }

    // ✅ BOLUM 1.1: Domain Method - Verify store
    public void Verify()
    {
        if (IsVerified)
            throw new DomainException("Mağaza zaten doğrulanmış");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Store Verified
        AddDomainEvent(new StoreVerifiedEvent(Id, SellerId));
    }

    // ✅ BOLUM 1.1: Domain Method - Suspend store
    public void Suspend(string? reason = null)
    {
        if (Status == EntityStatus.Suspended)
            throw new DomainException("Mağaza zaten askıya alınmış");

        if (Status == EntityStatus.Deleted)
            throw new DomainException("Silinmiş mağaza askıya alınamaz");

        Status = EntityStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Store Suspended
        AddDomainEvent(new StoreSuspendedEvent(Id, SellerId, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate store
    public void Activate()
    {
        if (Status == EntityStatus.Active)
            throw new DomainException("Mağaza zaten aktif");

        if (Status == EntityStatus.Deleted)
            throw new DomainException("Silinmiş mağaza aktifleştirilemez");

        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Store Activated
        AddDomainEvent(new StoreActivatedEvent(Id, SellerId));
    }

    // ✅ BOLUM 1.1: Domain Method - Set as primary store
    public void SetAsPrimary()
    {
        if (IsPrimary)
            throw new DomainException("Mağaza zaten birincil mağaza");

        if (Status != EntityStatus.Active)
            throw new DomainException("Sadece aktif mağaza birincil mağaza olarak ayarlanabilir");

        IsPrimary = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Store Set As Primary
        AddDomainEvent(new StoreSetAsPrimaryEvent(Id, SellerId));
    }

    // ✅ BOLUM 1.1: Domain Method - Remove primary status
    public void RemovePrimaryStatus()
    {
        if (!IsPrimary)
            return;

        IsPrimary = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Delete store (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Mağaza zaten silinmiş");

        IsDeleted = true;
        Status = EntityStatus.Deleted;
        IsPrimary = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Store Deleted
        AddDomainEvent(new StoreDeletedEvent(Id, SellerId));
    }

    // ✅ BOLUM 1.1: Helper Method - Generate slug from store name
    private static string GenerateSlug(string storeName)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new ArgumentException("Store name cannot be null or empty", nameof(storeName));

        return storeName
            .ToLowerInvariant()
            .Trim()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }
}
