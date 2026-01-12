using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// VolumeDiscount Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class VolumeDiscount : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    public Guid? CategoryId { get; private set; } // Category-wide discount
    public Guid? OrganizationId { get; private set; } // Organization-specific discount
    public int MinQuantity { get; private set; }
    public int? MaxQuantity { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - EF Core compatibility için decimal backing fields
    private decimal _discountPercentage;
    private decimal? _fixedDiscountAmount;
    
    // Database columns (EF Core mapping)
    public decimal DiscountPercentage 
    { 
        get => _discountPercentage; 
        private set 
        {
            // ✅ BOLUM 1.6: Invariant validation - Percentage 0-100 arası
            Guard.AgainstOutOfRange(value, 0m, 100m, nameof(DiscountPercentage));
            _discountPercentage = value;
        }
    }
    
    public decimal? FixedDiscountAmount 
    { 
        get => _fixedDiscountAmount; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegative(value.Value, nameof(FixedDiscountAmount));
            }
            _fixedDiscountAmount = value;
        }
    }
    
    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Percentage DiscountPercentageVO => new Percentage(_discountPercentage);
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? FixedDiscountAmountMoney => _fixedDiscountAmount.HasValue ? new Money(_fixedDiscountAmount.Value) : null;
    
    public bool IsActive { get; private set; } = true;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Product? Product { get; private set; }
    public Category? Category { get; private set; }
    public Organization? Organization { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private VolumeDiscount() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static VolumeDiscount Create(
        Guid productId,
        Product? product,
        Guid? categoryId,
        Category? category,
        Guid? organizationId,
        Organization? organization,
        int minQuantity,
        int? maxQuantity,
        decimal discountPercentage,
        decimal? fixedDiscountAmount = null,
        bool isActive = true,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Guard.AgainstNegativeOrZero(minQuantity, nameof(minQuantity));
        Guard.AgainstOutOfRange(discountPercentage, 0m, 100m, nameof(discountPercentage));

        // ✅ BOLUM 1.6: Invariant validation
        if (maxQuantity.HasValue && maxQuantity.Value < minQuantity)
            throw new DomainException("Maksimum miktar minimum miktardan küçük olamaz");

        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");

        if (discountPercentage > 0 && fixedDiscountAmount.HasValue && fixedDiscountAmount.Value > 0)
            throw new DomainException("Hem yüzde hem de sabit tutar indirimi aynı anda kullanılamaz");

        var volumeDiscount = new VolumeDiscount
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Product = product,
            CategoryId = categoryId,
            Category = category,
            OrganizationId = organizationId,
            Organization = organization,
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            DiscountPercentage = discountPercentage,
            FixedDiscountAmount = fixedDiscountAmount,
            IsActive = isActive,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

        return volumeDiscount;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update discount
    public void UpdateDiscount(decimal discountPercentage, decimal? fixedDiscountAmount = null)
    {
        Guard.AgainstOutOfRange(discountPercentage, 0m, 100m, nameof(discountPercentage));

        // ✅ BOLUM 1.6: Invariant validation
        if (discountPercentage > 0 && fixedDiscountAmount.HasValue && fixedDiscountAmount.Value > 0)
            throw new DomainException("Hem yüzde hem de sabit tutar indirimi aynı anda kullanılamaz");

        DiscountPercentage = discountPercentage;
        FixedDiscountAmount = fixedDiscountAmount;
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

