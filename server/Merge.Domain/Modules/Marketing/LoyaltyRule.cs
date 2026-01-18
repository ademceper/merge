using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// LoyaltyRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyRule : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public LoyaltyTransactionType Type { get; private set; }
    
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

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? MinimumPurchaseAmountMoney => _minimumPurchaseAmount.HasValue 
        ? new Money(_minimumPurchaseAmount.Value) 
        : null;

    private LoyaltyRule() { }

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

        var rule = new LoyaltyRule
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

        rule.AddDomainEvent(new LoyaltyRuleCreatedEvent(rule.Id, rule.Name, rule.Type, rule.PointsAwarded));

        return rule;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoyaltyRuleActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoyaltyRuleDeactivatedEvent(Id, Name));
    }

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

        if (minimumPurchaseAmount is not null)
            MinimumPurchaseAmount = minimumPurchaseAmount.Amount;

        if (expiryDays.HasValue)
            ExpiryDays = expiryDays.Value;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoyaltyRuleUpdatedEvent(Id, Name));
    }
}

