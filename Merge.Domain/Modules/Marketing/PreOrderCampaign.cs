using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// PreOrderCampaign Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class PreOrderCampaign : BaseAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime ExpectedDeliveryDate { get; private set; }
    public int MaxQuantity { get; private set; } // 0 = unlimited
    public int CurrentQuantity { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects - Percentage backing field (EF Core compatibility)
    private decimal _depositPercentage;
    public decimal DepositPercentage
    {
        get => _depositPercentage;
        private set
        {
            if (value < 0 || value > 100)
                throw new DomainException("Depozito yüzdesi 0-100 arasında olmalıdır");
            _depositPercentage = value;
        }
    }
    
    // ✅ BOLUM 1.3: Value Objects - Money backing field (EF Core compatibility)
    private decimal _specialPrice;
    public decimal SpecialPrice
    {
        get => _specialPrice;
        private set
        {
            if (value < 0)
                throw new DomainException("Özel fiyat negatif olamaz");
            _specialPrice = value;
        }
    }
    
    public bool IsActive { get; private set; }
    public bool NotifyOnAvailable { get; private set; }
    
    // ✅ BOLUM 1.4: Aggregate Root Pattern - PreOrder'lara sadece Campaign üzerinden erişim
    private readonly List<PreOrder> _preOrders = new();
    public IReadOnlyCollection<PreOrder> PreOrders => _preOrders.AsReadOnly();

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PreOrderCampaign() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PreOrderCampaign Create(
        string name,
        string description,
        Guid productId,
        DateTime startDate,
        DateTime endDate,
        DateTime expectedDeliveryDate,
        int maxQuantity,
        decimal depositPercentage,
        decimal specialPrice,
        bool notifyOnAvailable = true)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstDefault(productId, nameof(productId));
        
        if (endDate <= startDate)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
        if (expectedDeliveryDate <= endDate)
            throw new DomainException("Beklenen teslimat tarihi kampanya bitiş tarihinden sonra olmalıdır");
        if (maxQuantity < 0)
            throw new DomainException("Maksimum miktar negatif olamaz");
        if (depositPercentage < 0 || depositPercentage > 100)
            throw new DomainException("Depozito yüzdesi 0-100 arasında olmalıdır");
        if (specialPrice < 0)
            throw new DomainException("Özel fiyat negatif olamaz");

        return new PreOrderCampaign
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ProductId = productId,
            StartDate = startDate,
            EndDate = endDate,
            ExpectedDeliveryDate = expectedDeliveryDate,
            MaxQuantity = maxQuantity,
            CurrentQuantity = 0,
            DepositPercentage = depositPercentage,
            SpecialPrice = specialPrice,
            IsActive = true,
            NotifyOnAvailable = notifyOnAvailable,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment current quantity
    public void IncrementQuantity(int amount = 1)
    {
        if (amount <= 0)
            throw new DomainException("Miktar pozitif olmalıdır");

        if (MaxQuantity > 0 && CurrentQuantity + amount > MaxQuantity)
            throw new DomainException("Maksimum miktar aşıldı");

        CurrentQuantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Decrement current quantity
    public void DecrementQuantity(int amount = 1)
    {
        if (amount <= 0)
            throw new DomainException("Miktar pozitif olmalıdır");

        if (CurrentQuantity - amount < 0)
            throw new DomainException("Miktar negatif olamaz");

        CurrentQuantity -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if campaign is full
    public bool IsFull()
    {
        return MaxQuantity > 0 && CurrentQuantity >= MaxQuantity;
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if campaign is active
    public bool IsCurrentlyActive()
    {
        var now = DateTime.UtcNow;
        return IsActive && StartDate <= now && EndDate >= now;
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate campaign
    public void Activate()
    {
        if (IsActive)
            return; // Idempotent operation

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate campaign
    public void Deactivate()
    {
        if (!IsActive)
            return; // Idempotent operation

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update dates
    public void UpdateDates(DateTime startDate, DateTime endDate, DateTime expectedDeliveryDate)
    {
        if (endDate <= startDate)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
        if (expectedDeliveryDate <= endDate)
            throw new DomainException("Beklenen teslimat tarihi kampanya bitiş tarihinden sonra olmalıdır");

        StartDate = startDate;
        EndDate = endDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update pricing
    public void UpdatePricing(decimal depositPercentage, decimal specialPrice)
    {
        if (depositPercentage < 0 || depositPercentage > 100)
            throw new DomainException("Depozito yüzdesi 0-100 arasında olmalıdır");
        if (specialPrice < 0)
            throw new DomainException("Özel fiyat negatif olamaz");

        DepositPercentage = depositPercentage;
        SpecialPrice = specialPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update basic info
    public void UpdateBasicInfo(string name, string description, int maxQuantity)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        if (maxQuantity < 0)
            throw new DomainException("Maksimum miktar negatif olamaz");

        Name = name;
        Description = description;
        MaxQuantity = maxQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

