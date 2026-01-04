using Merge.Domain.Common;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Entities;

/// <summary>
/// WholesalePrice Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class WholesalePrice : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    public Guid? OrganizationId { get; private set; } // Organization-specific pricing
    public int MinQuantity { get; private set; } // Minimum quantity for this price tier
    public int? MaxQuantity { get; private set; } // Maximum quantity (null = unlimited)
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - EF Core compatibility için decimal backing field
    private decimal _price;
    
    // Database column (EF Core mapping)
    public decimal Price 
    { 
        get => _price; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Price));
            _price = value;
        }
    }
    
    // ✅ BOLUM 1.3: Value Object property (computed from decimal)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money PriceMoney => new Money(_price);
    
    public bool IsActive { get; private set; } = true;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Organization? Organization { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private WholesalePrice() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static WholesalePrice Create(
        Guid productId,
        Product product,
        Guid? organizationId,
        Organization? organization,
        int minQuantity,
        int? maxQuantity,
        decimal price,
        bool isActive = true,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNegativeOrZero(minQuantity, nameof(minQuantity));
        Guard.AgainstNegativeOrZero(price, nameof(price));

        // ✅ BOLUM 1.6: Invariant validation
        if (maxQuantity.HasValue && maxQuantity.Value < minQuantity)
            throw new DomainException("Maksimum miktar minimum miktardan küçük olamaz");

        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");

        var wholesalePrice = new WholesalePrice
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Product = product,
            OrganizationId = organizationId,
            Organization = organization,
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            Price = price,
            IsActive = isActive,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

        return wholesalePrice;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update price
    public void UpdatePrice(decimal newPrice)
    {
        Guard.AgainstNegativeOrZero(newPrice, nameof(newPrice));
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update quantity range
    public void UpdateQuantityRange(int minQuantity, int? maxQuantity)
    {
        Guard.AgainstNegativeOrZero(minQuantity, nameof(minQuantity));

        // ✅ BOLUM 1.6: Invariant validation
        if (maxQuantity.HasValue && maxQuantity.Value < minQuantity)
            throw new DomainException("Maksimum miktar minimum miktardan küçük olamaz");

        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate/Deactivate
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update dates
    public void UpdateDates(DateTime? startDate, DateTime? endDate)
    {
        // ✅ BOLUM 1.6: Invariant validation
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }
}

