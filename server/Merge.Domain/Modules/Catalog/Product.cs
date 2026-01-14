using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// Product aggregate root - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// </summary>
public class Product : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - SKU
    private string _sku = string.Empty;
    public string SKU 
    { 
        get => _sku; 
        private set => _sku = value; 
    }
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Money (EF Core compatibility için decimal backing)
    private decimal _price;
    private decimal? _discountPrice;
    
    public decimal Price 
    { 
        get => _price; 
        private set 
        { 
            Guard.AgainstNegativeOrZero(value, nameof(Price));
            _price = value;
        } 
    }
    
    public decimal? DiscountPrice 
    { 
        get => _discountPrice; 
        private set 
        { 
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(DiscountPrice));
                if (value.Value >= _price)
                    throw new DomainException("İndirimli fiyat normal fiyattan düşük olmalıdır");
            }
            _discountPrice = value;
        } 
    }
    
    // ✅ BOLUM 1.4: Invariant validation - StockQuantity >= 0
    private int _stockQuantity;
    public int StockQuantity 
    { 
        get => _stockQuantity; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(StockQuantity));
            _stockQuantity = value;
        } 
    }
    
    public string Brand { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access for ImageUrls
    private readonly List<string> _imageUrls = new();
    public IReadOnlyList<string> ImageUrls => _imageUrls.AsReadOnly();
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Rating (computed from reviews)
    private decimal _rating;
    public decimal Rating 
    { 
        get => _rating; 
        private set 
        { 
            Guard.AgainstOutOfRange(value, 0m, 5m, nameof(Rating));
            _rating = value;
        } 
    }
    
    public int ReviewCount { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;
    public Guid CategoryId { get; private set; }
    public Guid? SellerId { get; private set; }
    public Guid? StoreId { get; private set; }

    // ✅ BOLUM 1.5: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [NotMapped]
    public Money PriceMoney => new Money(_price);
    
    [NotMapped]
    public Money? DiscountPriceMoney => _discountPrice.HasValue ? new Money(_discountPrice.Value) : null;
    
    [NotMapped]
    public SKU SKUValueObject => new SKU(_sku);
    
    [NotMapped]
    public Rating RatingValueObject => new Rating((int)Math.Round(_rating));

    // Navigation properties
    public Category Category { get; private set; } = null!;
    public User? Seller { get; private set; }
    public Store? Store { get; private set; }
    
    // ✅ BOLUM 1.1: Encapsulated collections - Read-only access
    // Note: OrderItems, CartItems, FlashSaleProducts gibi cross-module navigation'lar
    // EF Core tarafından yönetildiği için ICollection olarak bırakıldı
    // Domain logic içinde bu collection'ları değiştirmek için domain method'lar kullanılmalı
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();
    
    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    
    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    
    private readonly List<Wishlist> _wishlists = new();
    public IReadOnlyCollection<Wishlist> Wishlists => _wishlists.AsReadOnly();
    
    public ICollection<FlashSaleProduct> FlashSaleProducts { get; private set; } = new List<FlashSaleProduct>();
    
    private readonly List<BundleItem> _bundleItems = new();
    public IReadOnlyCollection<BundleItem> BundleItems => _bundleItems.AsReadOnly();
    
    private readonly List<RecentlyViewedProduct> _recentlyViewedProducts = new();
    public IReadOnlyCollection<RecentlyViewedProduct> RecentlyViewedProducts => _recentlyViewedProducts.AsReadOnly();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Product() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Product Create(
        string name,
        string description,
        SKU sku,
        Money price,
        int stockQuantity,
        Guid categoryId,
        string brand,
        Guid? sellerId = null,
        Guid? storeId = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstOutOfRange(name.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(name));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstLength(description, ValidationConstants.MaxDescriptionLength, nameof(description));
        Guard.AgainstNull(sku, nameof(sku));
        Guard.AgainstNull(price, nameof(price));
        Guard.AgainstDefault(categoryId, nameof(categoryId));
        Guard.AgainstNullOrEmpty(brand, nameof(brand));
        Guard.AgainstLength(brand, ValidationConstants.MaxBrandLength, nameof(brand));

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            _sku = sku.Value,
            _price = price.Amount,
            _stockQuantity = stockQuantity,
            CategoryId = categoryId,
            Brand = brand,
            SellerId = sellerId,
            StoreId = storeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // ImageUrls boş başlar, sonra domain method'lar ile eklenir

        // ✅ BOLUM 1.4: Invariant validation
        product.ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - ProductCreatedEvent yayınla (ÖNERİLİR)
        product.AddDomainEvent(new ProductCreatedEvent(product.Id, name, sku.Value, categoryId, sellerId));

        return product;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set price
    public void SetPrice(Money newPrice)
    {
        Guard.AgainstNull(newPrice, nameof(newPrice));
        Guard.AgainstNegativeOrZero(newPrice.Amount, nameof(newPrice));

        if (_discountPrice.HasValue && newPrice.Amount <= _discountPrice.Value)
            throw new DomainException("Fiyat indirimli fiyattan düşük olamaz");

        _price = newPrice.Amount;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set discount price
    public void SetDiscountPrice(Money? discountPrice)
    {
        if (discountPrice == null)
        {
            _discountPrice = null;
            UpdatedAt = DateTime.UtcNow;
            
            // ✅ BOLUM 1.4: Invariant validation
            ValidateInvariants();
            
            // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
            return;
        }

        Guard.AgainstNegativeOrZero(discountPrice.Amount, nameof(discountPrice));

        if (discountPrice.Amount >= _price)
            throw new DomainException("İndirimli fiyat normal fiyattan düşük olmalıdır");

        _discountPrice = discountPrice.Amount;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Reduce stock
    public void ReduceStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (_stockQuantity < quantity)
            throw new DomainException($"Yetersiz stok. Mevcut: {_stockQuantity}, İstenen: {quantity}");

        _stockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Stok azalması önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Increase stock
    public void IncreaseStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        _stockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Stok artışı önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Reserve stock (for cart/order)
    public void ReserveStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (_stockQuantity < quantity)
            throw new DomainException($"Yetersiz stok. Mevcut: {_stockQuantity}, İstenen: {quantity}");

        // Stock reservation would be handled by Inventory entity in real scenario
        // For now, just check availability
    }

    // ✅ BOLUM 1.1: Domain Logic - Set stock quantity
    public void SetStockQuantity(int quantity)
    {
        Guard.AgainstNegative(quantity, nameof(quantity));
        _stockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Stok miktarı değişikliği önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate product
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Ürün aktif/pasif durumu önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate product
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Ürün aktif/pasif durumu önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update rating (called when review is added/updated)
    public void UpdateRating(decimal newRating, int reviewCount)
    {
        Guard.AgainstOutOfRange(newRating, 0m, 5m, nameof(newRating));
        Guard.AgainstNegative(reviewCount, nameof(reviewCount));

        _rating = newRating;
        ReviewCount = reviewCount;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Rating değişikliği önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update name
    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        Guard.AgainstOutOfRange(newName.Length, ValidationConstants.MinNameLength, ValidationConstants.MaxNameLength, nameof(newName));
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, newName, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update description
    public void UpdateDescription(string newDescription)
    {
        Guard.AgainstNullOrEmpty(newDescription, nameof(newDescription));
        Guard.AgainstLength(newDescription, ValidationConstants.MaxDescriptionLength, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update SKU
    public void UpdateSKU(SKU newSku)
    {
        Guard.AgainstNull(newSku, nameof(newSku));
        _sku = newSku.Value;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update brand
    public void UpdateBrand(string newBrand)
    {
        Guard.AgainstNullOrEmpty(newBrand, nameof(newBrand));
        Guard.AgainstLength(newBrand, ValidationConstants.MaxBrandLength, nameof(newBrand));
        Brand = newBrand;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update images
    public void UpdateImages(string imageUrl, List<string> imageUrls)
    {
        Guard.AgainstNullOrEmpty(imageUrl, nameof(imageUrl));
        Guard.AgainstLength(imageUrl, ValidationConstants.MaxImageUrlLength, nameof(imageUrl));
        Guard.AgainstNull(imageUrls, nameof(imageUrls));

        // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
        try
        {
            var urlValueObject = new Url(imageUrl);
        }
        catch (DomainException)
        {
            throw new DomainException("Geçersiz görsel URL formatı");
        }

        if (imageUrls.Count > ValidationConstants.MaxImageUrlsCount)
            throw new DomainException($"En fazla {ValidationConstants.MaxImageUrlsCount} görsel eklenebilir");

        foreach (var url in imageUrls)
        {
            Guard.AgainstLength(url, ValidationConstants.MaxImageUrlLength, nameof(imageUrls));
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(url);
            }
            catch (DomainException)
            {
                throw new DomainException($"Geçersiz görsel URL formatı: {url}");
            }
        }

        ImageUrl = imageUrl;
        _imageUrls.Clear();
        _imageUrls.AddRange(imageUrls);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add image URL
    public void AddImageUrl(string imageUrl)
    {
        Guard.AgainstNullOrEmpty(imageUrl, nameof(imageUrl));
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
        
        if (_imageUrls.Contains(imageUrl))
        {
            throw new DomainException("Bu görsel URL'i zaten eklenmiş");
        }
        
        if (_imageUrls.Count >= ValidationConstants.MaxImageUrlsCount)
        {
            throw new DomainException($"En fazla {ValidationConstants.MaxImageUrlsCount} görsel eklenebilir");
        }
        
        _imageUrls.Add(imageUrl);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove image URL
    public void RemoveImageUrl(string imageUrl)
    {
        Guard.AgainstNullOrEmpty(imageUrl, nameof(imageUrl));
        if (!_imageUrls.Remove(imageUrl))
        {
            throw new DomainException("Bu görsel URL'i bulunamadı");
        }
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set image URL
    public void SetImageUrl(string imageUrl)
    {
        Guard.AgainstNullOrEmpty(imageUrl, nameof(imageUrl));
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
        
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set category
    public void SetCategory(Guid categoryId)
    {
        Guard.AgainstDefault(categoryId, nameof(categoryId));
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, categoryId));
    }

    // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture - Domain katmanı Application'a bağımlı olmamalı)
    // Not: Application katmanındaki validator'larla senkronize tutulmalı
    private static class ValidationConstants
    {
        public const int MinNameLength = 2;
        public const int MaxNameLength = 200;
        public const int MaxDescriptionLength = 5000;
        public const int MaxBrandLength = 100;
        public const int MaxImageUrlLength = 2000;
        public const int MaxImageUrlsCount = 10; // Maksimum görsel sayısı
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Ürün adı boş olamaz");

        if (Name.Length < ValidationConstants.MinNameLength || Name.Length > ValidationConstants.MaxNameLength)
            throw new DomainException($"Ürün adı {ValidationConstants.MinNameLength}-{ValidationConstants.MaxNameLength} karakter arasında olmalıdır");

        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("Ürün açıklaması boş olamaz");

        if (Description.Length > ValidationConstants.MaxDescriptionLength)
            throw new DomainException($"Ürün açıklaması en fazla {ValidationConstants.MaxDescriptionLength} karakter olabilir");

        if (string.IsNullOrWhiteSpace(Brand))
            throw new DomainException("Marka boş olamaz");

        if (Brand.Length > ValidationConstants.MaxBrandLength)
            throw new DomainException($"Marka en fazla {ValidationConstants.MaxBrandLength} karakter olabilir");

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

        if (_imageUrls.Count > ValidationConstants.MaxImageUrlsCount)
            throw new DomainException($"En fazla {ValidationConstants.MaxImageUrlsCount} görsel eklenebilir");

        foreach (var url in _imageUrls)
        {
            // ✅ BOLUM 1.3: Value Objects - URL validation using Url Value Object
            try
            {
                var urlValueObject = new Url(url);
            }
            catch (DomainException)
            {
                throw new DomainException($"Geçersiz görsel URL formatı: {url}");
            }
        }

        if (_price <= 0)
            throw new DomainException("Ürün fiyatı pozitif olmalıdır");

        if (_stockQuantity < 0)
            throw new DomainException("Stok miktarı negatif olamaz");

        if (_discountPrice.HasValue && _discountPrice.Value >= _price)
            throw new DomainException("İndirimli fiyat normal fiyattan düşük olmalıdır");
    }

    // ✅ BOLUM 1.1: Business Logic - Check if product is available
    public bool IsAvailable(int requestedQuantity = 1)
    {
        return IsActive && _stockQuantity >= requestedQuantity;
    }

    // ✅ BOLUM 1.1: Business Logic - Get current price (discount or regular)
    public Money GetCurrentPrice()
    {
        return _discountPrice.HasValue ? new Money(_discountPrice.Value) : new Money(_price);
    }

    // ✅ BOLUM 1.1: Business Logic - Check if product has discount
    public bool HasDiscount()
    {
        return _discountPrice.HasValue && _discountPrice.Value < _price;
    }

    // ✅ BOLUM 1.1: Business Logic - Get discount percentage
    public Percentage? GetDiscountPercentage()
    {
        if (!HasDiscount())
            return null;

        var discountAmount = _price - _discountPrice!.Value;
        var percentage = (discountAmount / _price) * 100;
        return new Percentage(percentage);
    }

    // ✅ BOLUM 1.1: Domain Logic - Add review (collection manipulation)
    public void AddReview(Review review)
    {
        Guard.AgainstNull(review, nameof(review));
        if (review.ProductId != Id)
        {
            throw new DomainException("Review bu ürüne ait değil");
        }
        if (_reviews.Any(r => r.Id == review.Id))
        {
            throw new DomainException("Bu review zaten eklenmiş");
        }
        _reviews.Add(review);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Review ekleme önemli bir business event'tir (rating hesaplaması için)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove review (collection manipulation)
    public void RemoveReview(Guid reviewId)
    {
        Guard.AgainstDefault(reviewId, nameof(reviewId));
        var review = _reviews.FirstOrDefault(r => r.Id == reviewId);
        if (review == null)
        {
            throw new DomainException("Review bulunamadı");
        }
        _reviews.Remove(review);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Review silme önemli bir business event'tir (rating hesaplaması için)
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add variant (collection manipulation)
    public void AddVariant(ProductVariant variant)
    {
        Guard.AgainstNull(variant, nameof(variant));
        if (variant.ProductId != Id)
        {
            throw new DomainException("Variant bu ürüne ait değil");
        }
        if (_variants.Any(v => v.Id == variant.Id))
        {
            throw new DomainException("Bu variant zaten eklenmiş");
        }
        _variants.Add(variant);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Variant ekleme önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove variant (collection manipulation)
    public void RemoveVariant(Guid variantId)
    {
        Guard.AgainstDefault(variantId, nameof(variantId));
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
        {
            throw new DomainException("Variant bulunamadı");
        }
        _variants.Remove(variant);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductUpdatedEvent yayınla (ÖNERİLİR)
        // Variant silme önemli bir business event'tir
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, _sku, CategoryId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductDeletedEvent(Id, Name, _sku));
    }
}

