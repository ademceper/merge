using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductVariant Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class ProductVariant : BaseEntity
{
    // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture)
    private static class ValidationConstants
    {
        public const int MaxNameLength = 100;
        public const int MaxValueLength = 100;
        public const int MaxImageUrlLength = 2000;
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Name));
            Guard.AgainstLength(value, ValidationConstants.MaxNameLength, nameof(Name));
            _name = value;
        } 
    }
    
    private string _value = string.Empty;
    public string Value 
    { 
        get => _value; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Value));
            Guard.AgainstLength(value, ValidationConstants.MaxValueLength, nameof(Value));
            _value = value;
        } 
    }
    
    private string? _sku;
    public string? SKU 
    { 
        get => _sku; 
        private set => _sku = value; 
    }
    
    private decimal? _price;
    public decimal? Price 
    { 
        get => _price; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(Price));
            }
            _price = value;
        } 
    }
    
    private int _stockQuantity = 0;
    public int StockQuantity 
    { 
        get => _stockQuantity; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(StockQuantity));
            _stockQuantity = value;
        } 
    }
    
    public string? ImageUrl { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // ✅ BOLUM 1.3: Value Object properties (computed from nullable types)
    [NotMapped]
    public SKU? SKUValueObject => _sku != null ? new SKU(_sku) : null;
    
    [NotMapped]
    public Money? PriceMoney => _price.HasValue ? new Money(_price.Value) : null;
    
    // Navigation properties
    public Product Product { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductVariant() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductVariant Create(
        Guid productId,
        string name,
        string value,
        string? sku = null,
        decimal? price = null,
        int stockQuantity = 0,
        string? imageUrl = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(value, nameof(value));
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            Guard.AgainstLength(imageUrl, ValidationConstants.MaxImageUrlLength, nameof(imageUrl));
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(imageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }
        
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            _name = name,
            _value = value,
            _sku = sku,
            _price = price,
            _stockQuantity = stockQuantity,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.4: Invariant validation
        variant.ValidateInvariants();
        
        return variant;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update variant details
    public void Update(
        string name,
        string value,
        string? sku = null,
        decimal? price = null,
        string? imageUrl = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, ValidationConstants.MaxNameLength, nameof(name));
        Guard.AgainstNullOrEmpty(value, nameof(value));
        Guard.AgainstLength(value, ValidationConstants.MaxValueLength, nameof(value));
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            Guard.AgainstLength(imageUrl, ValidationConstants.MaxImageUrlLength, nameof(imageUrl));
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(imageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }
        
        _name = name;
        _value = value;
        _sku = sku;
        _price = price;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update stock quantity
    public void UpdateStockQuantity(int newStockQuantity)
    {
        Guard.AgainstNegative(newStockQuantity, nameof(newStockQuantity));
        _stockQuantity = newStockQuantity;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update price
    public void UpdatePrice(decimal? newPrice)
    {
        if (newPrice.HasValue)
        {
            Guard.AgainstNegativeOrZero(newPrice.Value, nameof(newPrice));
        }
        _price = newPrice;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (string.IsNullOrWhiteSpace(_name))
            throw new DomainException("Varyant adı boş olamaz");

        Guard.AgainstLength(_name, ValidationConstants.MaxNameLength, nameof(Name));

        if (string.IsNullOrWhiteSpace(_value))
            throw new DomainException("Varyant değeri boş olamaz");

        Guard.AgainstLength(_value, ValidationConstants.MaxValueLength, nameof(Value));

        if (!string.IsNullOrEmpty(ImageUrl))
        {
            Guard.AgainstLength(ImageUrl, ValidationConstants.MaxImageUrlLength, nameof(ImageUrl));
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(ImageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }

        if (_sku != null)
        {
            try
            {
                var skuValueObject = new SKU(_sku); // Validation için
            }
            catch (ArgumentException)
            {
                throw new DomainException("Geçersiz SKU formatı");
            }
        }

        if (_price.HasValue && _price.Value <= 0)
            throw new DomainException("Varyant fiyatı pozitif olmalıdır");

        if (_stockQuantity < 0)
            throw new DomainException("Varyant stok miktarı negatif olamaz");
    }
}

