using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductBundle Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class ProductBundle : BaseEntity, IAggregateRoot
{
    private static class ValidationConstants
    {
        public const int MinNameLength = 2;
        public const int MaxNameLength = 200;
        public const int MaxDescriptionLength = 5000;
        public const int MaxImageUrlLength = 2000;
    }

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
    
    private string _description = string.Empty;
    public string Description 
    { 
        get => _description; 
        private set 
        {
            Guard.AgainstLength(value, ValidationConstants.MaxDescriptionLength, nameof(Description));
            _description = value ?? string.Empty;
        } 
    }
    
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
    
    private string _imageUrl = string.Empty;
    public string ImageUrl 
    { 
        get => _imageUrl; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, ValidationConstants.MaxImageUrlLength, nameof(ImageUrl));
            }
            _imageUrl = value ?? string.Empty;
        } 
    }
    public bool IsActive { get; private set; } = true;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    [NotMapped]
    public Money BundlePriceMoney => new Money(_bundlePrice);
    
    [NotMapped]
    public Money? OriginalTotalPriceMoney => _originalTotalPrice.HasValue ? new Money(_originalTotalPrice.Value) : null;
    
    [NotMapped]
    public Percentage DiscountPercentageValueObject => new Percentage(_discountPercentage);
    
    // Navigation properties
    private readonly List<BundleItem> _bundleItems = new();
    public IReadOnlyCollection<BundleItem> BundleItems => _bundleItems.AsReadOnly();
    
    private ProductBundle() { }
    
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
        Guard.AgainstOutOfRange(name.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(name));
        Guard.AgainstNegativeOrZero(bundlePrice, nameof(bundlePrice));
        
        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, ValidationConstants.MaxDescriptionLength, nameof(description));
        }
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            Guard.AgainstLength(imageUrl, ValidationConstants.MaxImageUrlLength, nameof(imageUrl));
            try
            {
                var urlValueObject = new Url(imageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }
        
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
            _name = name,
            _description = description ?? string.Empty,
            _bundlePrice = bundlePrice,
            _originalTotalPrice = originalTotalPrice,
            _discountPercentage = discountPercentage,
            _imageUrl = imageUrl ?? string.Empty,
            IsActive = true,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };
        
        bundle.ValidateInvariants();
        
        bundle.AddDomainEvent(new ProductBundleCreatedEvent(bundle.Id, name, bundlePrice, discountPercentage));
        
        return bundle;
    }
    
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
        Guard.AgainstOutOfRange(name.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(name));
        
        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, ValidationConstants.MaxDescriptionLength, nameof(description));
        }
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            Guard.AgainstLength(imageUrl, ValidationConstants.MaxImageUrlLength, nameof(imageUrl));
            try
            {
                var urlValueObject = new Url(imageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }
        
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
        {
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");
        }
        
        Name = name;
        if (description is not null) Description = description;
        if (bundlePrice.HasValue) BundlePrice = bundlePrice.Value;
        if (originalTotalPrice.HasValue) OriginalTotalPrice = originalTotalPrice.Value;
        if (imageUrl is not null) ImageUrl = imageUrl;
        if (startDate.HasValue) StartDate = startDate;
        if (endDate.HasValue) EndDate = endDate;
        
        // Recalculate discount percentage
        if (originalTotalPrice.HasValue && originalTotalPrice.Value > 0 && bundlePrice.HasValue)
        {
            _discountPercentage = ((originalTotalPrice.Value - bundlePrice.Value) / originalTotalPrice.Value) * 100;
        }
        
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    public void AddItem(Guid productId, int quantity, int sortOrder = 0)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNegative(sortOrder, nameof(sortOrder));
        
        if (_bundleItems.Any(bi => bi.ProductId == productId))
        {
            throw new DomainException("Bu ürün zaten pakete eklenmiş");
        }
        
        var item = BundleItem.Create(Id, productId, quantity, sortOrder);
        
        _bundleItems.Add(item);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        // Ürün ekleme önemli bir business event'tir
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    public void RemoveItem(Guid productId)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        
        var item = _bundleItems.FirstOrDefault(bi => bi.ProductId == productId);
        if (item is null)
        {
            throw new DomainException("Bu ürün pakette bulunamadı");
        }
        
        _bundleItems.Remove(item);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        // Ürün çıkarma önemli bir business event'tir
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
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
        
        ValidateInvariants();
        
        // Fiyat güncellemesi önemli bir business event'tir
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductBundleUpdatedEvent(Id, Name, BundlePrice));
    }
    
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductBundleDeletedEvent(Id, Name));
    }

    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new DomainException("Bundle adı boş olamaz");

        Guard.AgainstOutOfRange(_name.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(Name));

        if (!string.IsNullOrEmpty(_description))
        {
            Guard.AgainstLength(_description, ValidationConstants.MaxDescriptionLength, nameof(Description));
        }

        if (!string.IsNullOrEmpty(_imageUrl))
        {
            Guard.AgainstLength(_imageUrl, ValidationConstants.MaxImageUrlLength, nameof(ImageUrl));
            try
            {
                var urlValueObject = new Url(_imageUrl);
            }
            catch (DomainException)
            {
                throw new DomainException("Geçersiz görsel URL formatı");
            }
        }

        if (_bundlePrice <= 0)
            throw new DomainException("Bundle fiyatı pozitif olmalıdır");

        if (_discountPercentage < 0 || _discountPercentage > 100)
            throw new DomainException("İndirim yüzdesi 0-100 arasında olmalıdır");

        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");
    }
}

