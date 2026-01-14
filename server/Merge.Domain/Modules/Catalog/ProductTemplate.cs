using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductTemplate Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class ProductTemplate : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture)
    private static class ValidationConstants
    {
        public const int MinNameLength = 2;
        public const int MaxNameLength = 200;
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Name));
            Guard.AgainstOutOfRange(value.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(Name));
            _name = value;
        } 
    }
    
    public string Description { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public string? Brand { get; private set; }
    public string? DefaultSKUPrefix { get; private set; }
    
    private decimal? _defaultPrice;
    public decimal? DefaultPrice 
    { 
        get => _defaultPrice; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(DefaultPrice));
            }
            _defaultPrice = value;
        } 
    }
    
    private int? _defaultStockQuantity;
    public int? DefaultStockQuantity 
    { 
        get => _defaultStockQuantity; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegative(value.Value, nameof(DefaultStockQuantity));
            }
            _defaultStockQuantity = value;
        } 
    }
    
    public string? DefaultImageUrl { get; private set; }
    public string? Specifications { get; private set; } // JSON for default specifications
    public string? Attributes { get; private set; } // JSON for default attributes
    public bool IsActive { get; private set; } = true;
    
    private int _usageCount = 0;
    public int UsageCount 
    { 
        get => _usageCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(UsageCount));
            _usageCount = value;
        } 
    }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Category Category { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductTemplate() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductTemplate Create(
        string name,
        string description,
        Guid categoryId,
        string? brand = null,
        string? defaultSKUPrefix = null,
        decimal? defaultPrice = null,
        int? defaultStockQuantity = null,
        string? defaultImageUrl = null,
        string? specifications = null,
        string? attributes = null,
        bool isActive = true)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstOutOfRange(name.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(name));
        Guard.AgainstDefault(categoryId, nameof(categoryId));
        
        if (!string.IsNullOrEmpty(defaultImageUrl))
        {
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(defaultImageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }
        
        var template = new ProductTemplate
        {
            Id = Guid.NewGuid(),
            _name = name,
            Description = description ?? string.Empty,
            CategoryId = categoryId,
            Brand = brand,
            DefaultSKUPrefix = defaultSKUPrefix,
            _defaultPrice = defaultPrice,
            _defaultStockQuantity = defaultStockQuantity,
            DefaultImageUrl = defaultImageUrl,
            Specifications = specifications,
            Attributes = attributes,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.4: Invariant validation
        template.ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        template.AddDomainEvent(new ProductTemplateCreatedEvent(template.Id, name, categoryId));
        
        return template;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update
    public void Update(
        string? name = null,
        string? description = null,
        Guid? categoryId = null,
        string? brand = null,
        string? defaultSKUPrefix = null,
        decimal? defaultPrice = null,
        int? defaultStockQuantity = null,
        string? defaultImageUrl = null,
        string? specifications = null,
        string? attributes = null,
        bool? isActive = null)
    {
        if (!string.IsNullOrEmpty(name)) Name = name;
        if (description != null) Description = description;
        if (categoryId.HasValue) CategoryId = categoryId.Value;
        if (brand != null) Brand = brand;
        if (defaultSKUPrefix != null) DefaultSKUPrefix = defaultSKUPrefix;
        if (defaultPrice.HasValue) DefaultPrice = defaultPrice;
        if (defaultStockQuantity.HasValue) DefaultStockQuantity = defaultStockQuantity;
        if (defaultImageUrl != null)
        {
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(defaultImageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
            DefaultImageUrl = defaultImageUrl;
        }
        if (specifications != null) Specifications = specifications;
        if (attributes != null) Attributes = attributes;
        if (isActive.HasValue) IsActive = isActive.Value;
        
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new ProductTemplateUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Increment usage count
    public void IncrementUsageCount()
    {
        _usageCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductTemplateUpdatedEvent yayınla (ÖNERİLİR)
        // Usage count değişikliği önemli bir business event'tir
        AddDomainEvent(new ProductTemplateUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Activate
    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductTemplateUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductTemplateUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Deactivate
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductTemplateUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductTemplateUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new ProductTemplateDeletedEvent(Id, Name));
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new DomainException("Şablon adı boş olamaz");

        Guard.AgainstOutOfRange(_name.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(Name));

        if (Guid.Empty == CategoryId)
            throw new DomainException("Kategori ID boş olamaz");

        if (_defaultPrice.HasValue && _defaultPrice.Value <= 0)
            throw new DomainException("Varsayılan fiyat pozitif olmalıdır");

        if (_defaultStockQuantity.HasValue && _defaultStockQuantity.Value < 0)
            throw new DomainException("Varsayılan stok miktarı negatif olamaz");

        if (_usageCount < 0)
            throw new DomainException("Kullanım sayısı negatif olamaz");

        if (!string.IsNullOrEmpty(DefaultImageUrl))
        {
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(DefaultImageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }
    }
}

