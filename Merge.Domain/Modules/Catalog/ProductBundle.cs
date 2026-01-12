using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductBundle Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class ProductBundle : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.3: Value Objects - Money backing fields (EF Core compatibility)
    private decimal _bundlePrice;
    public decimal BundlePrice 
    { 
        get => _bundlePrice; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(BundlePrice));
            _bundlePrice = value;
        } 
    }
    
    private decimal? _originalTotalPrice;
    public decimal? OriginalTotalPrice 
    { 
        get => _originalTotalPrice; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(OriginalTotalPrice));
            }
            _originalTotalPrice = value;
        } 
    }
    
    private decimal _discountPercentage;
    public decimal DiscountPercentage 
    { 
        get => _discountPercentage; 
        private set 
        {
            Guard.AgainstOutOfRange(value, 0m, 100m, nameof(DiscountPercentage));
            _discountPercentage = value;
        } 
    }
    
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    
    // Navigation properties
    private readonly List<BundleItem> _bundleItems = new();
    public IReadOnlyCollection<BundleItem> BundleItems => _bundleItems.AsReadOnly();
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductBundle() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductBundle Create(
        string name,
        string description,
        decimal bundlePrice,
        decimal? originalTotalPrice,
        string? imageUrl = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNegativeOrZero(bundlePrice, nameof(bundlePrice));
        
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
        {
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");
        }
        
        var discountPercentage = originalTotalPrice.HasValue && originalTotalPrice.Value > 0
            ? ((originalTotalPrice.Value - bundlePrice) / originalTotalPrice.Value) * 100
            : 0m;
        
        var bundle = new ProductBundle
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            _bundlePrice = bundlePrice,
            _originalTotalPrice = originalTotalPrice,
            _discountPercentage = discountPercentage,
            ImageUrl = imageUrl ?? string.Empty,
            IsActive = true,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.5: Domain Events
        bundle.AddDomainEvent(new ProductBundleCreatedEvent(bundle.Id, name, bundlePrice, discountPercentage));
        
        return bundle;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update bundle
    public void Update(
        string name,
        string? description = null,
        decimal? bundlePrice = null,
        decimal? originalTotalPrice = null,
        string? imageUrl = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
        {
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");
        }
        
        Name = name;
        if (description != null) Description = description;
        if (bundlePrice.HasValue) BundlePrice = bundlePrice.Value;
        if (originalTotalPrice.HasValue) OriginalTotalPrice = originalTotalPrice.Value;
        if (imageUrl != null) ImageUrl = imageUrl;
        if (startDate.HasValue) StartDate = startDate;
        if (endDate.HasValue) EndDate = endDate;
        
        // Recalculate discount percentage
        if (originalTotalPrice.HasValue && originalTotalPrice.Value > 0 && bundlePrice.HasValue)
        {
            _discountPercentage = ((originalTotalPrice.Value - bundlePrice.Value) / originalTotalPrice.Value) * 100;
        }
        
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add item
    public void AddItem(Guid productId, int quantity, int sortOrder = 0)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNegative(sortOrder, nameof(sortOrder));
        
        if (_bundleItems.Any(bi => bi.ProductId == productId))
        {
            throw new DomainException("Bu ürün zaten pakete eklenmiş");
        }
        
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var item = BundleItem.Create(Id, productId, quantity, sortOrder);
        
        _bundleItems.Add(item);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - ProductBundleUpdatedEvent yayınla (ÖNERİLİR)
        // Ürün ekleme önemli bir business event'tir
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove item
    public void RemoveItem(Guid productId)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        
        var item = _bundleItems.FirstOrDefault(bi => bi.ProductId == productId);
        if (item == null)
        {
            throw new DomainException("Bu ürün pakette bulunamadı");
        }
        
        _bundleItems.Remove(item);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - ProductBundleUpdatedEvent yayınla (ÖNERİLİR)
        // Ürün çıkarma önemli bir business event'tir
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update total prices (recalculates discount percentage)
    public void UpdateTotalPrices(decimal? originalTotalPrice)
    {
        if (originalTotalPrice.HasValue)
        {
            Guard.AgainstNegativeOrZero(originalTotalPrice.Value, nameof(originalTotalPrice));
            _originalTotalPrice = originalTotalPrice.Value;
            
            // Recalculate discount percentage
            if (_originalTotalPrice.Value > 0)
            {
                _discountPercentage = ((_originalTotalPrice.Value - _bundlePrice) / _originalTotalPrice.Value) * 100;
            }
        }
        else
        {
            _originalTotalPrice = null;
            _discountPercentage = 0m;
        }
        
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - ProductBundleUpdatedEvent yayınla (ÖNERİLİR)
        // Fiyat güncellemesi önemli bir business event'tir
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Activate
    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - ProductBundleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Deactivate
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - ProductBundleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new ProductBundleDeletedEvent(Id, Name));
    }
}

