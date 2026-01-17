---
paths:
  - "Merge.Domain/**/*.cs"
  - "**/Modules/**/*.cs"
  - "**/Entities/**/*.cs"
  - "**/ValueObjects/**/*.cs"
alwaysApply: false
---

# DOMAIN-DRIVEN DESIGN RULES

> Bu dosya DDD pattern kurallarını içerir.
> CLAUDE: Domain entity oluştururken bu kurallara MUTLAKA uy!

---

## 1. DOMAIN LAYER STRUCTURE

```
Merge.Domain/
├── Modules/                     # Bounded Contexts
│   ├── Catalog/                 # Product catalog bounded context
│   │   ├── Product.cs          # Aggregate Root
│   │   ├── Category.cs         # Aggregate Root
│   │   ├── ProductImage.cs     # Entity (owned by Product)
│   │   ├── ProductAttribute.cs # Entity (owned by Product)
│   │   └── ProductTag.cs       # Entity (owned by Product)
│   ├── Ordering/               # Order management bounded context
│   │   ├── Order.cs           # Aggregate Root
│   │   ├── OrderItem.cs       # Entity (owned by Order)
│   │   └── OrderStatus.cs     # Enum
│   ├── Identity/              # User management bounded context
│   │   ├── User.cs           # Aggregate Root
│   │   ├── Role.cs           # Aggregate Root
│   │   └── RefreshToken.cs   # Entity
│   ├── Payments/              # Payment bounded context
│   │   ├── Payment.cs        # Aggregate Root
│   │   └── PaymentMethod.cs  # Enum
│   ├── Shipping/              # Shipping bounded context
│   │   └── Shipment.cs       # Aggregate Root
│   └── Marketing/             # Marketing bounded context
│       ├── Campaign.cs       # Aggregate Root
│       └── Coupon.cs         # Aggregate Root
├── ValueObjects/              # Shared value objects
│   ├── Money.cs
│   ├── Address.cs
│   ├── Email.cs
│   ├── PhoneNumber.cs
│   ├── SKU.cs
│   └── DateRange.cs
├── SharedKernel/              # Shared abstractions
│   ├── BaseEntity.cs
│   ├── BaseAggregateRoot.cs
│   ├── IAggregateRoot.cs
│   ├── IDomainEvent.cs
│   └── Guard.cs
├── Specifications/            # Query specifications
│   └── BaseSpecification.cs
├── Enums/                     # Business enums
│   ├── OrderStatus.cs
│   ├── PaymentStatus.cs
│   └── UserStatus.cs
├── Exceptions/                # Domain exceptions
│   ├── DomainException.cs
│   ├── NotFoundException.cs
│   └── ValidationException.cs
└── Events/                    # Domain events
    ├── ProductEvents.cs
    ├── OrderEvents.cs
    └── UserEvents.cs
```

---

## 2. BASE CLASSES

### 2.1 BaseEntity

```csharp
namespace Merge.Domain.SharedKernel;

/// <summary>
/// Base class for all domain entities.
/// Provides common properties and domain event support.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    public byte[]? RowVersion { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this entity.
    /// Events are dispatched after SaveChanges via Outbox pattern.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks entity as updated. Called automatically by domain methods.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft delete. Preserves data for audit/recovery.
    /// </summary>
    public virtual void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Entity is already deleted");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Restore soft-deleted entity.
    /// </summary>
    public virtual void Restore()
    {
        if (!IsDeleted)
            throw new DomainException("Entity is not deleted");

        IsDeleted = false;
        DeletedAt = null;
        MarkAsUpdated();
    }
}
```

### 2.2 BaseAggregateRoot

```csharp
namespace Merge.Domain.SharedKernel;

/// <summary>
/// Base class for aggregate roots.
/// Aggregate roots are the entry point to an aggregate and ensure consistency.
/// </summary>
public abstract class BaseAggregateRoot : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Tenant ID for multi-tenant support.
    /// </summary>
    public Guid? TenantId { get; protected set; }

    /// <summary>
    /// User who created this aggregate.
    /// </summary>
    public Guid? CreatedBy { get; protected set; }

    /// <summary>
    /// User who last updated this aggregate.
    /// </summary>
    public Guid? UpdatedBy { get; protected set; }
}

/// <summary>
/// Marker interface for aggregate roots.
/// </summary>
public interface IAggregateRoot
{
    Guid Id { get; }
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

### 2.3 Domain Event Interface

```csharp
namespace Merge.Domain.SharedKernel;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something significant that happened in the domain.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }
}

/// <summary>
/// Base record for domain events.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
```

---

## 3. AGGREGATE ROOT EXAMPLE: PRODUCT

```csharp
namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// Product aggregate root.
/// Manages product information, pricing, inventory, and related entities.
/// </summary>
public class Product : BaseAggregateRoot
{
    // ============================================
    // PROPERTIES (private setters - encapsulation)
    // ============================================

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public SKU SKU { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? SellerId { get; private set; }

    // Navigation properties
    public Category Category { get; private set; } = null!;
    public Seller? Seller { get; private set; }

    // Collections (private backing field + public readonly)
    private readonly List<ProductAttribute> _attributes = [];
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<ProductTag> _tags = [];
    public IReadOnlyCollection<ProductTag> Tags => _tags.AsReadOnly();

    private readonly List<ProductImage> _images = [];
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    // ============================================
    // CONSTRUCTORS
    // ============================================

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Product() { }

    /// <summary>
    /// Private constructor with all required parameters.
    /// Used by factory method.
    /// </summary>
    private Product(
        string name,
        string? description,
        SKU sku,
        Money price,
        int stockQuantity,
        Guid categoryId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        SKU = sku;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // ============================================
    // FACTORY METHOD (ONLY way to create)
    // ============================================

    /// <summary>
    /// Creates a new product. This is the ONLY way to create a Product.
    /// </summary>
    /// <param name="name">Product name (required, max 200 chars)</param>
    /// <param name="description">Product description (optional)</param>
    /// <param name="sku">Stock Keeping Unit (validated by SKU value object)</param>
    /// <param name="price">Price (validated by Money value object)</param>
    /// <param name="stockQuantity">Initial stock quantity (must be non-negative)</param>
    /// <param name="categoryId">Category ID (required)</param>
    /// <returns>New Product instance</returns>
    /// <exception cref="ArgumentException">When validation fails</exception>
    public static Product Create(
        string name,
        string? description,
        SKU sku,
        Money price,
        int stockQuantity,
        Guid categoryId)
    {
        // Guard clauses - fail fast
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 200, nameof(name));
        Guard.AgainstNull(sku, nameof(sku));
        Guard.AgainstNull(price, nameof(price));
        Guard.AgainstNegative(stockQuantity, nameof(stockQuantity));
        Guard.AgainstDefault(categoryId, nameof(categoryId));

        var product = new Product(name, description, sku, price, stockQuantity, categoryId);

        // Raise domain event
        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.SKU.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.CategoryId));

        return product;
    }

    // ============================================
    // DOMAIN METHODS (business behavior)
    // ============================================

    /// <summary>
    /// Updates product name.
    /// </summary>
    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 200, nameof(name));

        if (Name == name) return;

        var oldName = Name;
        Name = name;
        MarkAsUpdated();

        AddDomainEvent(new ProductNameChangedEvent(Id, oldName, name));
    }

    /// <summary>
    /// Updates product description.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Guard.AgainstLength(description, 5000, nameof(description));

        Description = description;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates product details (name and description together).
    /// </summary>
    public void UpdateDetails(string name, string? description)
    {
        UpdateName(name);
        UpdateDescription(description);
    }

    /// <summary>
    /// Sets the product price.
    /// </summary>
    public void SetPrice(Money newPrice)
    {
        Guard.AgainstNull(newPrice, nameof(newPrice));

        if (newPrice.Amount < 0)
            throw new DomainException("Price cannot be negative");

        if (Price.Equals(newPrice)) return;

        var oldPrice = Price;
        Price = newPrice;
        MarkAsUpdated();

        AddDomainEvent(new ProductPriceChangedEvent(
            Id,
            oldPrice.Amount,
            oldPrice.Currency,
            newPrice.Amount,
            newPrice.Currency));
    }

    /// <summary>
    /// Sets the stock quantity directly.
    /// Use for inventory adjustments, not for order processing.
    /// </summary>
    public void SetStock(int quantity)
    {
        Guard.AgainstNegative(quantity, nameof(quantity));

        if (StockQuantity == quantity) return;

        var oldStock = StockQuantity;
        StockQuantity = quantity;
        MarkAsUpdated();

        AddDomainEvent(new ProductStockChangedEvent(Id, oldStock, quantity));

        // Check if became out of stock
        if (oldStock > 0 && quantity == 0)
        {
            AddDomainEvent(new ProductOutOfStockEvent(Id, Name));
        }
    }

    /// <summary>
    /// Increases stock quantity (e.g., restocking).
    /// </summary>
    public void IncreaseStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        var oldStock = StockQuantity;
        StockQuantity += quantity;
        MarkAsUpdated();

        AddDomainEvent(new ProductStockChangedEvent(Id, oldStock, StockQuantity));

        // Check if came back in stock
        if (oldStock == 0 && StockQuantity > 0)
        {
            AddDomainEvent(new ProductBackInStockEvent(Id, Name));
        }
    }

    /// <summary>
    /// Decreases stock quantity (e.g., order placed).
    /// </summary>
    /// <exception cref="DomainException">When insufficient stock</exception>
    public void DecreaseStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (StockQuantity < quantity)
            throw new DomainException(
                $"Insufficient stock for product '{Name}'. " +
                $"Requested: {quantity}, Available: {StockQuantity}");

        var oldStock = StockQuantity;
        StockQuantity -= quantity;
        MarkAsUpdated();

        AddDomainEvent(new ProductStockChangedEvent(Id, oldStock, StockQuantity));

        if (StockQuantity == 0)
        {
            AddDomainEvent(new ProductOutOfStockEvent(Id, Name));
        }
    }

    /// <summary>
    /// Reserves stock for an order (doesn't decrease yet).
    /// </summary>
    public void ReserveStock(int quantity, Guid orderId)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (StockQuantity < quantity)
            throw new DomainException(
                $"Cannot reserve {quantity} units of '{Name}'. Only {StockQuantity} available.");

        AddDomainEvent(new ProductStockReservedEvent(Id, orderId, quantity));
    }

    /// <summary>
    /// Changes the product category.
    /// </summary>
    public void ChangeCategory(Guid newCategoryId)
    {
        Guard.AgainstDefault(newCategoryId, nameof(newCategoryId));

        if (CategoryId == newCategoryId) return;

        var oldCategoryId = CategoryId;
        CategoryId = newCategoryId;
        MarkAsUpdated();

        AddDomainEvent(new ProductCategoryChangedEvent(Id, oldCategoryId, newCategoryId));
    }

    /// <summary>
    /// Assigns a seller to the product (for marketplace).
    /// </summary>
    public void SetSeller(Guid sellerId)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));

        SellerId = sellerId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the product (makes it visible/purchasable).
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Product is already active");

        if (StockQuantity == 0)
            throw new DomainException("Cannot activate product with zero stock");

        IsActive = true;
        MarkAsUpdated();

        AddDomainEvent(new ProductActivatedEvent(Id, Name));
    }

    /// <summary>
    /// Deactivates the product (hides from catalog).
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Product is already inactive");

        IsActive = false;
        MarkAsUpdated();

        AddDomainEvent(new ProductDeactivatedEvent(Id, Name));
    }

    // ============================================
    // COLLECTION MANAGEMENT
    // ============================================

    /// <summary>
    /// Adds an attribute to the product.
    /// </summary>
    public void AddAttribute(string key, string value)
    {
        Guard.AgainstNullOrEmpty(key, nameof(key));
        Guard.AgainstNullOrEmpty(value, nameof(value));

        var existingAttribute = _attributes.FirstOrDefault(a => a.Key == key);
        if (existingAttribute is not null)
        {
            existingAttribute.UpdateValue(value);
        }
        else
        {
            _attributes.Add(ProductAttribute.Create(Id, key, value));
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Removes an attribute from the product.
    /// </summary>
    public void RemoveAttribute(string key)
    {
        var attribute = _attributes.FirstOrDefault(a => a.Key == key);
        if (attribute is not null)
        {
            _attributes.Remove(attribute);
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Adds a tag to the product.
    /// </summary>
    public void AddTag(string tag)
    {
        Guard.AgainstNullOrEmpty(tag, nameof(tag));

        var normalizedTag = tag.ToLowerInvariant().Trim();

        if (_tags.Any(t => t.Value == normalizedTag))
            return; // Already exists

        _tags.Add(ProductTag.Create(Id, normalizedTag));
        MarkAsUpdated();
    }

    /// <summary>
    /// Removes a tag from the product.
    /// </summary>
    public void RemoveTag(string tag)
    {
        var normalizedTag = tag.ToLowerInvariant().Trim();
        var existingTag = _tags.FirstOrDefault(t => t.Value == normalizedTag);

        if (existingTag is not null)
        {
            _tags.Remove(existingTag);
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Adds an image to the product.
    /// </summary>
    public ProductImage AddImage(string url, int displayOrder = 0, bool isMain = false)
    {
        Guard.AgainstNullOrEmpty(url, nameof(url));

        // If this is set as main, unset other main images
        if (isMain)
        {
            foreach (var img in _images.Where(i => i.IsMain))
            {
                img.SetAsNotMain();
            }
        }

        // If no images exist, this becomes main
        if (_images.Count == 0)
        {
            isMain = true;
        }

        var image = ProductImage.Create(Id, url, displayOrder, isMain);
        _images.Add(image);
        MarkAsUpdated();

        return image;
    }

    /// <summary>
    /// Removes an image from the product.
    /// </summary>
    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            throw new NotFoundException("ProductImage", imageId);

        var wasMain = image.IsMain;
        _images.Remove(image);

        // If removed image was main, set first remaining as main
        if (wasMain && _images.Count > 0)
        {
            _images.First().SetAsMain();
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Sets an image as the main image.
    /// </summary>
    public void SetMainImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new NotFoundException("ProductImage", imageId);

        foreach (var img in _images)
        {
            if (img.Id == imageId)
                img.SetAsMain();
            else
                img.SetAsNotMain();
        }

        MarkAsUpdated();
    }
}
```

---

## 4. CHILD ENTITY EXAMPLE

```csharp
namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// Product attribute entity.
/// Owned by Product aggregate.
/// </summary>
public class ProductAttribute : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    private ProductAttribute() { }

    private ProductAttribute(Guid productId, string key, string value)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Key = key;
        Value = value;
        CreatedAt = DateTime.UtcNow;
    }

    internal static ProductAttribute Create(Guid productId, string key, string value)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNullOrEmpty(key, nameof(key));
        Guard.AgainstNullOrEmpty(value, nameof(value));
        Guard.AgainstLength(key, 100, nameof(key));
        Guard.AgainstLength(value, 500, nameof(value));

        return new ProductAttribute(productId, key, value);
    }

    internal void UpdateValue(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));
        Guard.AgainstLength(value, 500, nameof(value));

        Value = value;
        MarkAsUpdated();
    }
}

/// <summary>
/// Product image entity.
/// Owned by Product aggregate.
/// </summary>
public class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = null!;
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }

    private ProductImage() { }

    private ProductImage(Guid productId, string url, int displayOrder, bool isMain)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Url = url;
        DisplayOrder = displayOrder;
        IsMain = isMain;
        CreatedAt = DateTime.UtcNow;
    }

    internal static ProductImage Create(Guid productId, string url, int displayOrder, bool isMain)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNullOrEmpty(url, nameof(url));

        return new ProductImage(productId, url, displayOrder, isMain);
    }

    internal void SetAsMain()
    {
        IsMain = true;
        MarkAsUpdated();
    }

    internal void SetAsNotMain()
    {
        IsMain = false;
        MarkAsUpdated();
    }

    internal void UpdateDisplayOrder(int order)
    {
        DisplayOrder = order;
        MarkAsUpdated();
    }
}
```

---

## 5. VALUE OBJECTS

### 5.1 Money

```csharp
namespace Merge.Domain.ValueObjects;

/// <summary>
/// Money value object.
/// Immutable, self-validating, with rich behavior.
/// </summary>
public sealed record Money : IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private static readonly string[] SupportedCurrencies = ["TRY", "USD", "EUR", "GBP"];

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a Money instance.
    /// </summary>
    public static Money Create(decimal amount, string currency)
    {
        Guard.AgainstNegative(amount, nameof(amount));
        Guard.AgainstNullOrEmpty(currency, nameof(currency));

        var normalizedCurrency = currency.ToUpperInvariant();

        if (!SupportedCurrencies.Contains(normalizedCurrency))
            throw new DomainException($"Unsupported currency: {currency}. Supported: {string.Join(", ", SupportedCurrencies)}");

        return new Money(Math.Round(amount, 2), normalizedCurrency);
    }

    /// <summary>
    /// Creates zero money in the specified currency.
    /// </summary>
    public static Money Zero(string currency) => Create(0, currency);

    /// <summary>
    /// Adds two Money values.
    /// </summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another Money value.
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;

        if (result < 0)
            throw new DomainException("Subtraction would result in negative amount");

        return Create(result, Currency);
    }

    /// <summary>
    /// Multiplies by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new DomainException("Cannot multiply by negative factor");

        return Create(Amount * factor, Currency);
    }

    /// <summary>
    /// Calculates percentage of this amount.
    /// </summary>
    public Money Percentage(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new DomainException("Percentage must be between 0 and 100");

        return Create(Amount * (percentage / 100), Currency);
    }

    /// <summary>
    /// Applies a discount percentage.
    /// </summary>
    public Money ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new DomainException("Discount percentage must be between 0 and 100");

        var discount = Amount * (discountPercentage / 100);
        return Create(Amount - discount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot perform operation on different currencies: {Currency} and {other.Currency}");
    }

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);

    public override string ToString() => $"{Amount:N2} {Currency}";
}
```

### 5.2 Email

```csharp
namespace Merge.Domain.ValueObjects;

/// <summary>
/// Email value object.
/// Validates email format and normalizes to lowercase.
/// </summary>
public sealed record Email
{
    public string Value { get; }

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));
        Guard.AgainstLength(value, 254, nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new DomainException($"Invalid email format: {value}");

        return new Email(normalized);
    }

    public string GetDomain() => Value.Split('@')[1];
    public string GetLocalPart() => Value.Split('@')[0];

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
```

### 5.3 Address

```csharp
namespace Merge.Domain.ValueObjects;

/// <summary>
/// Address value object.
/// Represents a complete mailing address.
/// </summary>
public sealed record Address
{
    public string Street { get; }
    public string? Street2 { get; }
    public string City { get; }
    public string? State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(
        string street,
        string? street2,
        string city,
        string? state,
        string postalCode,
        string country)
    {
        Street = street;
        Street2 = street2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(
        string street,
        string city,
        string postalCode,
        string country,
        string? street2 = null,
        string? state = null)
    {
        Guard.AgainstNullOrEmpty(street, nameof(street));
        Guard.AgainstLength(street, 200, nameof(street));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstLength(city, 100, nameof(city));
        Guard.AgainstNullOrEmpty(postalCode, nameof(postalCode));
        Guard.AgainstNullOrEmpty(country, nameof(country));

        if (street2 is not null)
            Guard.AgainstLength(street2, 200, nameof(street2));

        if (state is not null)
            Guard.AgainstLength(state, 100, nameof(state));

        return new Address(
            street.Trim(),
            street2?.Trim(),
            city.Trim(),
            state?.Trim(),
            postalCode.Trim(),
            country.Trim().ToUpperInvariant());
    }

    public string ToSingleLine()
    {
        var parts = new List<string> { Street };
        if (!string.IsNullOrEmpty(Street2)) parts.Add(Street2);
        parts.Add(City);
        if (!string.IsNullOrEmpty(State)) parts.Add(State);
        parts.Add(PostalCode);
        parts.Add(Country);
        return string.Join(", ", parts);
    }

    public override string ToString() => ToSingleLine();
}
```

### 5.4 SKU

```csharp
namespace Merge.Domain.ValueObjects;

/// <summary>
/// Stock Keeping Unit value object.
/// Validates SKU format: uppercase letters, numbers, and hyphens.
/// </summary>
public sealed record SKU
{
    public string Value { get; }

    private static readonly Regex SkuRegex = new(
        @"^[A-Z0-9-]{3,50}$",
        RegexOptions.Compiled);

    private SKU(string value)
    {
        Value = value;
    }

    public static SKU Create(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));

        var normalized = value.Trim().ToUpperInvariant();

        if (!SkuRegex.IsMatch(normalized))
            throw new DomainException(
                $"Invalid SKU format: {value}. " +
                "SKU must be 3-50 characters, containing only uppercase letters, numbers, and hyphens.");

        return new SKU(normalized);
    }

    /// <summary>
    /// Generates a SKU from a prefix and sequence number.
    /// </summary>
    public static SKU Generate(string prefix, int sequence)
    {
        Guard.AgainstNullOrEmpty(prefix, nameof(prefix));
        Guard.AgainstNegative(sequence, nameof(sequence));

        var sku = $"{prefix.ToUpperInvariant()}-{sequence:D6}";
        return Create(sku);
    }

    public override string ToString() => Value;

    public static implicit operator string(SKU sku) => sku.Value;
}
```

### 5.5 DateRange

```csharp
namespace Merge.Domain.ValueObjects;

/// <summary>
/// Date range value object.
/// Useful for promotions, campaigns, availability periods.
/// </summary>
public sealed record DateRange
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new DomainException("End date cannot be before start date");

        return new DateRange(startDate, endDate);
    }

    public static DateRange CreateFromToday(int days)
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(days);
        return new DateRange(start, end);
    }

    public bool Contains(DateTime date) => date >= StartDate && date <= EndDate;

    public bool IsActive() => Contains(DateTime.UtcNow);

    public bool HasStarted() => DateTime.UtcNow >= StartDate;

    public bool HasEnded() => DateTime.UtcNow > EndDate;

    public int TotalDays => (EndDate - StartDate).Days;

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}";
}
```

---

## 6. GUARD CLAUSES

```csharp
namespace Merge.Domain.SharedKernel;

/// <summary>
/// Guard clauses for validating invariants.
/// Fail fast with meaningful exceptions.
/// </summary>
public static class Guard
{
    public static void AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName, $"{parameterName} cannot be null");
    }

    public static void AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
    }

    public static void AgainstDefault<T>(T value, string parameterName)
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new ArgumentException($"{parameterName} cannot be default value", parameterName);
    }

    public static void AgainstNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException($"{parameterName} cannot be negative", parameterName);
    }

    public static void AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException($"{parameterName} cannot be negative", parameterName);
    }

    public static void AgainstNegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentException($"{parameterName} must be positive", parameterName);
    }

    public static void AgainstNegativeOrZero(decimal value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentException($"{parameterName} must be positive", parameterName);
    }

    public static void AgainstLength(string? value, int maxLength, string parameterName)
    {
        if (value is not null && value.Length > maxLength)
            throw new ArgumentException(
                $"{parameterName} cannot exceed {maxLength} characters (was {value.Length})",
                parameterName);
    }

    public static void AgainstOutOfRange<T>(T value, T min, T max, string parameterName)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"{parameterName} must be between {min} and {max}");
    }

    public static void AgainstEmpty<T>(IEnumerable<T>? collection, string parameterName)
    {
        if (collection is null || !collection.Any())
            throw new ArgumentException($"{parameterName} cannot be empty", parameterName);
    }

    public static void Against(bool condition, string message)
    {
        if (condition)
            throw new DomainException(message);
    }

    public static void Against<TException>(bool condition, string message)
        where TException : Exception, new()
    {
        if (condition)
        {
            var exception = (TException)Activator.CreateInstance(typeof(TException), message)!;
            throw exception;
        }
    }
}
```

---

## 7. DOMAIN EVENTS

```csharp
namespace Merge.Domain.Events;

// ============================================
// PRODUCT EVENTS
// ============================================

public record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    string SKU,
    decimal Price,
    string Currency,
    Guid CategoryId
) : DomainEvent;

public record ProductNameChangedEvent(
    Guid ProductId,
    string OldName,
    string NewName
) : DomainEvent;

public record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    string OldCurrency,
    decimal NewPrice,
    string NewCurrency
) : DomainEvent;

public record ProductStockChangedEvent(
    Guid ProductId,
    int OldStock,
    int NewStock
) : DomainEvent;

public record ProductOutOfStockEvent(
    Guid ProductId,
    string ProductName
) : DomainEvent;

public record ProductBackInStockEvent(
    Guid ProductId,
    string ProductName
) : DomainEvent;

public record ProductStockReservedEvent(
    Guid ProductId,
    Guid OrderId,
    int Quantity
) : DomainEvent;

public record ProductCategoryChangedEvent(
    Guid ProductId,
    Guid OldCategoryId,
    Guid NewCategoryId
) : DomainEvent;

public record ProductActivatedEvent(
    Guid ProductId,
    string ProductName
) : DomainEvent;

public record ProductDeactivatedEvent(
    Guid ProductId,
    string ProductName
) : DomainEvent;

// ============================================
// ORDER EVENTS
// ============================================

public record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    string Currency
) : DomainEvent;

public record OrderItemAddedEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
) : DomainEvent;

public record OrderSubmittedEvent(
    Guid OrderId,
    decimal TotalAmount,
    string Currency
) : DomainEvent;

public record OrderConfirmedEvent(
    Guid OrderId
) : DomainEvent;

public record OrderShippedEvent(
    Guid OrderId,
    string TrackingNumber,
    string Carrier
) : DomainEvent;

public record OrderDeliveredEvent(
    Guid OrderId,
    DateTime DeliveredAt
) : DomainEvent;

public record OrderCancelledEvent(
    Guid OrderId,
    string Reason
) : DomainEvent;

// ============================================
// USER EVENTS
// ============================================

public record UserRegisteredEvent(
    Guid UserId,
    string Email
) : DomainEvent;

public record UserEmailVerifiedEvent(
    Guid UserId
) : DomainEvent;

public record UserPasswordChangedEvent(
    Guid UserId
) : DomainEvent;

public record UserLockedOutEvent(
    Guid UserId,
    DateTime LockoutEnd
) : DomainEvent;
```

---

## 8. DOMAIN RULES SUMMARY

### Entity Rules
1. **Private setters** - ASLA property'leri dışarıdan set etme
2. **Factory methods** - SADECE `Create()` ile entity oluştur
3. **Guard clauses** - Tüm invariant'ları valide et
4. **Domain events** - Önemli state değişikliklerinde event raise et
5. **Rich behavior** - Business logic entity içinde olmalı

### Aggregate Rules
1. **Single transaction** - Bir aggregate = bir transaction
2. **Reference by ID** - Diğer aggregate'lere ID ile referans ver
3. **Consistency boundary** - Aggregate içinde tutarlılık sağla
4. **Small aggregates** - Küçük, odaklı aggregate'ler tercih et

### Value Object Rules
1. **Immutable** - Oluşturulduktan sonra DEĞİŞMEZ
2. **Self-validating** - Factory'de valide et
3. **No identity** - Tüm property'lerle equality
4. **Rich behavior** - Domain operasyonlarını içer

### Domain Event Rules
1. **Past tense** - Geçmiş zaman kullan (Created, Changed, Deleted)
2. **Immutable** - Record type kullan
3. **All data included** - Handler'ın ihtiyacı olan tüm veriyi içer
4. **Raised in entity** - Entity method'larından raise et

---

## DDD CHECKLIST

Her Entity için:
- [ ] Private constructor var (EF Core için)
- [ ] Factory method var (Create)
- [ ] Tüm setter'lar private
- [ ] Guard clauses kullanılıyor
- [ ] Domain events raise ediliyor
- [ ] Business logic entity içinde

Her Value Object için:
- [ ] Record type kullanılmış
- [ ] Private constructor var
- [ ] Static Create factory method var
- [ ] Validation factory'de yapılıyor
- [ ] Immutable (tüm property'ler get-only)
