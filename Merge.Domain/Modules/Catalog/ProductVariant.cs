using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductVariant Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class ProductVariant : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Name));
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
            _value = value;
        } 
    }
    
    public string? SKU { get; private set; }
    
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
        
        return new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            _name = name,
            _value = value,
            SKU = sku,
            _price = price,
            _stockQuantity = stockQuantity,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };
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
        Guard.AgainstNullOrEmpty(value, nameof(value));
        
        _name = name;
        _value = value;
        SKU = sku;
        _price = price;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update stock quantity
    public void UpdateStockQuantity(int newStockQuantity)
    {
        Guard.AgainstNegative(newStockQuantity, nameof(newStockQuantity));
        _stockQuantity = newStockQuantity;
        UpdatedAt = DateTime.UtcNow;
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
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

