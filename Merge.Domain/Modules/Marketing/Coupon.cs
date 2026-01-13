using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// Coupon aggregate root - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class Coupon : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Money, Percentage
    private decimal _discountAmount;
    private decimal? _discountPercentage;
    private decimal? _minimumPurchaseAmount;
    private decimal? _maximumDiscountAmount;
    
    public decimal DiscountAmount 
    { 
        get => _discountAmount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(DiscountAmount));
            _discountAmount = value;
        } 
    }
    
    public decimal? DiscountPercentage 
    { 
        get => _discountPercentage; 
        private set 
        { 
            if (value.HasValue)
            {
                Guard.AgainstOutOfRange(value.Value, 0m, 100m, nameof(DiscountPercentage));
            }
            _discountPercentage = value;
        } 
    }
    
    public decimal? MinimumPurchaseAmount 
    { 
        get => _minimumPurchaseAmount; 
        private set 
        { 
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(MinimumPurchaseAmount));
            _minimumPurchaseAmount = value;
        } 
    }
    
    public decimal? MaximumDiscountAmount 
    { 
        get => _maximumDiscountAmount; 
        private set 
        { 
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(MaximumDiscountAmount));
            _maximumDiscountAmount = value;
        } 
    }
    
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    
    // ✅ BOLUM 1.4: Invariant validation - UsageCount <= MaxUsage
    private int _usageLimit;
    private int _usedCount;
    
    public int UsageLimit 
    { 
        get => _usageLimit; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(UsageLimit));
            _usageLimit = value;
        } 
    }
    
    public int UsedCount 
    { 
        get => _usedCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(UsedCount));
            // ✅ BOLUM 1.4: Invariant - UsageCount cannot exceed UsageLimit
            if (_usageLimit > 0 && value > _usageLimit)
                throw new DomainException($"Kullanım sayısı limiti aşılamaz. Limit: {_usageLimit}");
            _usedCount = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;
    public bool IsForNewUsersOnly { get; private set; } = false;
    
    // ✅ BOLUM 1.1: Rich Domain Model - Backing fields for encapsulated collections
    private List<Guid>? _applicableCategoryIds;
    private List<Guid>? _applicableProductIds;
    
    public List<Guid>? ApplicableCategoryIds 
    { 
        get => _applicableCategoryIds; 
        private set => _applicableCategoryIds = value; 
    }
    
    public List<Guid>? ApplicableProductIds 
    { 
        get => _applicableProductIds; 
        private set => _applicableProductIds = value; 
    }

    // ✅ BOLUM 1.5: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.3: Value Object properties
    [NotMapped]
    public Money DiscountAmountMoney => new Money(_discountAmount);
    
    [NotMapped]
    public Percentage? DiscountPercentageValue => _discountPercentage.HasValue 
        ? new Percentage(_discountPercentage.Value) 
        : null;
    
    [NotMapped]
    public Money? MinimumPurchaseAmountMoney => _minimumPurchaseAmount.HasValue 
        ? new Money(_minimumPurchaseAmount.Value) 
        : null;
    
    [NotMapped]
    public Money? MaximumDiscountAmountMoney => _maximumDiscountAmount.HasValue 
        ? new Money(_maximumDiscountAmount.Value) 
        : null;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Coupon() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Coupon Create(
        string code,
        string description,
        Money? discountAmount,
        Percentage? discountPercentage,
        DateTime startDate,
        DateTime endDate,
        int usageLimit = 0,
        Money? minimumPurchaseAmount = null,
        Money? maximumDiscountAmount = null,
        bool isForNewUsersOnly = false)
    {
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstNullOrEmpty(description, nameof(description));

        if (discountAmount == null && discountPercentage == null)
            throw new DomainException("Kupon için indirim tutarı veya yüzdesi belirtilmelidir");

        if (startDate >= endDate)
            throw new DomainException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant(),
            Description = description,
            _discountAmount = discountAmount?.Amount ?? 0,
            _discountPercentage = discountPercentage?.Value,
            StartDate = startDate,
            EndDate = endDate,
            _usageLimit = usageLimit,
            _minimumPurchaseAmount = minimumPurchaseAmount?.Amount,
            _maximumDiscountAmount = maximumDiscountAmount?.Amount,
            IsForNewUsersOnly = isForNewUsersOnly,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - CouponCreatedEvent
        coupon.AddDomainEvent(new CouponCreatedEvent(coupon.Id, coupon.Code, coupon.DiscountAmount, coupon.DiscountPercentage));

        return coupon;
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment usage count
    public void IncrementUsage()
    {
        // ✅ BOLUM 1.4: Invariant validation - UsageCount <= MaxUsage
        if (_usageLimit > 0 && _usedCount >= _usageLimit)
            throw new DomainException("Kupon kullanım limitine ulaşıldı");

        if (!IsActive)
            throw new DomainException("Kupon aktif değil");

        if (DateTime.UtcNow < StartDate || DateTime.UtcNow > EndDate)
            throw new DomainException("Kupon geçerli tarih aralığında değil");

        _usedCount++;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CouponUsedEvent
        AddDomainEvent(new CouponUsedEvent(Id, Code, _usedCount, _usageLimit));
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if coupon is valid
    public bool IsValid()
    {
        if (!IsActive)
            return false;

        if (DateTime.UtcNow < StartDate || DateTime.UtcNow > EndDate)
            return false;

        if (_usageLimit > 0 && _usedCount >= _usageLimit)
            return false;

        return true;
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if coupon can be used for amount
    public bool CanBeUsedFor(decimal purchaseAmount)
    {
        if (!IsValid())
            return false;

        if (_minimumPurchaseAmount.HasValue && purchaseAmount < _minimumPurchaseAmount.Value)
            return false;

        return true;
    }

    // ✅ BOLUM 1.1: Domain Logic - Calculate discount for purchase amount
    public Money CalculateDiscount(Money purchaseAmount)
    {
        if (!CanBeUsedFor(purchaseAmount.Amount))
            throw new DomainException("Kupon bu alışveriş için kullanılamaz");

        Money discount;

        if (_discountPercentage.HasValue)
        {
            var percentage = new Percentage(_discountPercentage.Value);
            var calculatedDiscount = percentage.ApplyTo(purchaseAmount.Amount);
            discount = new Money(calculatedDiscount);
        }
        else
        {
            discount = new Money(_discountAmount);
        }

        // Apply maximum discount limit if exists
        if (_maximumDiscountAmount.HasValue && discount.Amount > _maximumDiscountAmount.Value)
            discount = new Money(_maximumDiscountAmount.Value);

        return discount;
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate coupon
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CouponActivatedEvent
        AddDomainEvent(new CouponActivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate coupon
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CouponDeactivatedEvent
        AddDomainEvent(new CouponDeactivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update code
    public void UpdateCode(string newCode)
    {
        Guard.AgainstNullOrEmpty(newCode, nameof(newCode));
        Code = newCode.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update description
    public void UpdateDescription(string newDescription)
    {
        Guard.AgainstNullOrEmpty(newDescription, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set discount amount
    public void SetDiscountAmount(Money? discountAmount)
    {
        _discountAmount = discountAmount?.Amount ?? 0;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set discount percentage
    public void SetDiscountPercentage(Percentage? discountPercentage)
    {
        _discountPercentage = discountPercentage?.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set minimum purchase amount
    public void SetMinimumPurchaseAmount(Money? minimumPurchaseAmount)
    {
        _minimumPurchaseAmount = minimumPurchaseAmount?.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set maximum discount amount
    public void SetMaximumDiscountAmount(Money? maximumDiscountAmount)
    {
        _maximumDiscountAmount = maximumDiscountAmount?.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set usage limit
    public void SetUsageLimit(int usageLimit)
    {
        Guard.AgainstNegative(usageLimit, nameof(usageLimit));
        _usageLimit = usageLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set applicable category IDs
    public void SetApplicableCategoryIds(List<Guid>? categoryIds)
    {
        _applicableCategoryIds = categoryIds;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set applicable product IDs
    public void SetApplicableProductIds(List<Guid>? productIds)
    {
        _applicableProductIds = productIds;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set for new users only
    public void SetForNewUsersOnly(bool isForNewUsersOnly)
    {
        IsForNewUsersOnly = isForNewUsersOnly;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update dates
    public void UpdateDates(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            throw new DomainException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CouponDeletedEvent
        AddDomainEvent(new CouponDeletedEvent(Id, Code));
    }
}

