using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// LoyaltyRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyRule : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public LoyaltyTransactionType Type { get; private set; }
    
    // ✅ BOLUM 1.6: Invariant validation - PointsAwarded >= 0
    private int _pointsAwarded;
    public int PointsAwarded 
    { 
        get => _pointsAwarded; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(PointsAwarded));
            _pointsAwarded = value;
        } 
    }
    
    // ✅ BOLUM 1.3: Value Objects - Money backing field (EF Core compatibility)
    private decimal? _minimumPurchaseAmount;
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
    
    // ✅ BOLUM 1.6: Invariant validation - ExpiryDays > 0
    private int? _expiryDays;
    public int? ExpiryDays 
    { 
        get => _expiryDays; 
        private set 
        {
            if (value.HasValue)
                Guard.AgainstNegativeOrZero(value.Value, nameof(ExpiryDays));
            _expiryDays = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;

    // ✅ BOLUM 1.3: Value Object properties
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? MinimumPurchaseAmountMoney => _minimumPurchaseAmount.HasValue 
        ? new Money(_minimumPurchaseAmount.Value) 
        : null;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LoyaltyRule() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LoyaltyRule Create(
        string name,
        string description,
        LoyaltyTransactionType type,
        int pointsAwarded,
        Money? minimumPurchaseAmount = null,
        int? expiryDays = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstNegative(pointsAwarded, nameof(pointsAwarded));

        return new LoyaltyRule
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            _pointsAwarded = pointsAwarded,
            _minimumPurchaseAmount = minimumPurchaseAmount?.Amount,
            _expiryDays = expiryDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update details
    public void UpdateDetails(
        string? name = null,
        string? description = null,
        int? pointsAwarded = null,
        Money? minimumPurchaseAmount = null,
        int? expiryDays = null)
    {
        if (!string.IsNullOrEmpty(name))
            Name = name;

        if (!string.IsNullOrEmpty(description))
            Description = description;

        if (pointsAwarded.HasValue)
            PointsAwarded = pointsAwarded.Value;

        if (minimumPurchaseAmount != null)
            MinimumPurchaseAmount = minimumPurchaseAmount.Amount;

        if (expiryDays.HasValue)
            ExpiryDays = expiryDays.Value;

        UpdatedAt = DateTime.UtcNow;
    }
}

