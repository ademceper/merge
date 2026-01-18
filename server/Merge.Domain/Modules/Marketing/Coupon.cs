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
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
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
            if (_usageLimit > 0 && value > _usageLimit)
                throw new DomainException($"Kullanım sayısı limiti aşılamaz. Limit: {_usageLimit}");
            _usedCount = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;
    public bool IsForNewUsersOnly { get; private set; } = false;
    
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

    [Timestamp]
    public byte[]? RowVersion { get; set; }

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

    private Coupon() { }

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

        if (discountAmount is null && discountPercentage is null)
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

        coupon.AddDomainEvent(new CouponCreatedEvent(coupon.Id, coupon.Code, coupon.DiscountAmount, coupon.DiscountPercentage));

        return coupon;
    }

    public void IncrementUsage()
    {
        if (_usageLimit > 0 && _usedCount >= _usageLimit)
            throw new DomainException("Kupon kullanım limitine ulaşıldı");

        if (!IsActive)
            throw new DomainException("Kupon aktif değil");

        if (DateTime.UtcNow < StartDate || DateTime.UtcNow > EndDate)
            throw new DomainException("Kupon geçerli tarih aralığında değil");

        _usedCount++;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CouponUsedEvent(Id, Code, _usedCount, _usageLimit));
    }

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

    public bool CanBeUsedFor(decimal purchaseAmount)
    {
        if (!IsValid())
            return false;

        if (_minimumPurchaseAmount.HasValue && purchaseAmount < _minimumPurchaseAmount.Value)
            return false;

        return true;
    }

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

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CouponActivatedEvent(Id, Code));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CouponDeactivatedEvent(Id, Code));
    }

    public void UpdateCode(string newCode)
    {
        Guard.AgainstNullOrEmpty(newCode, nameof(newCode));
        Code = newCode.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string newDescription)
    {
        Guard.AgainstNullOrEmpty(newDescription, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDiscountAmount(Money? discountAmount)
    {
        _discountAmount = discountAmount?.Amount ?? 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDiscountPercentage(Percentage? discountPercentage)
    {
        _discountPercentage = discountPercentage?.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMinimumPurchaseAmount(Money? minimumPurchaseAmount)
    {
        _minimumPurchaseAmount = minimumPurchaseAmount?.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMaximumDiscountAmount(Money? maximumDiscountAmount)
    {
        _maximumDiscountAmount = maximumDiscountAmount?.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetUsageLimit(int usageLimit)
    {
        Guard.AgainstNegative(usageLimit, nameof(usageLimit));
        _usageLimit = usageLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetApplicableCategoryIds(List<Guid>? categoryIds)
    {
        _applicableCategoryIds = categoryIds;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetApplicableProductIds(List<Guid>? productIds)
    {
        _applicableProductIds = productIds;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetForNewUsersOnly(bool isForNewUsersOnly)
    {
        IsForNewUsersOnly = isForNewUsersOnly;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDates(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            throw new DomainException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CouponDeletedEvent(Id, Code));
    }
}

