using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// SubscriptionPlan Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionPlan : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    public SubscriptionPlanType PlanType { get; private set; } = SubscriptionPlanType.Monthly;
    
    private decimal _price;
    public decimal Price 
    { 
        get => _price; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(Price));
            _price = value;
        } 
    }
    
    public int DurationDays { get; private set; } // How many days the subscription lasts
    public int? TrialDays { get; private set; } // Free trial period
    public string? Features { get; private set; } // JSON string for plan features
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; } = 0;
    
    public BillingCycle BillingCycle { get; private set; } = BillingCycle.Monthly;
    public int MaxUsers { get; private set; } = 1; // Maximum users allowed
    
    private decimal? _setupFee;
    public decimal? SetupFee 
    { 
        get => _setupFee; 
        private set 
        {
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(SetupFee));
            _setupFee = value;
        } 
    }
    
    public string Currency { get; private set; } = "TRY";

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money PriceMoney => new Money(_price, Currency);
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? SetupFeeMoney => _setupFee.HasValue ? new Money(_setupFee.Value, Currency) : null;

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public ICollection<UserSubscription> UserSubscriptions { get; private set; } = [];

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(
        string name,
        string description,
        SubscriptionPlanType planType,
        decimal price,
        int durationDays,
        BillingCycle billingCycle,
        int maxUsers = 1,
        int? trialDays = null,
        decimal? setupFee = null,
        string currency = "TRY",
        string? features = null,
        bool isActive = true,
        int displayOrder = 0)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 100, nameof(name));
        Guard.AgainstLength(description, 1000, nameof(description));
        Guard.AgainstNegativeOrZero(price, nameof(price));
        Guard.AgainstNegativeOrZero(durationDays, nameof(durationDays));
        Guard.AgainstNegativeOrZero(maxUsers, nameof(maxUsers));
        
        if (trialDays.HasValue && trialDays.Value < 0)
            throw new DomainException("Trial days cannot be negative");
        
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter code");

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            PlanType = planType,
            _price = price,
            DurationDays = durationDays,
            TrialDays = trialDays,
            Features = features,
            IsActive = isActive,
            DisplayOrder = displayOrder,
            BillingCycle = billingCycle,
            MaxUsers = maxUsers,
            _setupFee = setupFee,
            Currency = currency.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        plan.AddDomainEvent(new SubscriptionPlanCreatedEvent(plan.Id, name, planType, price, currency));

        return plan;
    }

    public void Update(
        string? name = null,
        string? description = null,
        decimal? price = null,
        int? durationDays = null,
        int? trialDays = null,
        string? features = null,
        bool? isActive = null,
        int? displayOrder = null,
        BillingCycle? billingCycle = null,
        int? maxUsers = null,
        decimal? setupFee = null,
        string? currency = null)
    {
        if (name is not null)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstLength(name, 100, nameof(name));
            Name = name;
        }

        if (description is not null)
        {
            Guard.AgainstLength(description, 1000, nameof(description));
            Description = description;
        }

        if (price.HasValue)
        {
            Guard.AgainstNegativeOrZero(price.Value, nameof(price));
            _price = price.Value;
        }

        if (durationDays.HasValue)
        {
            Guard.AgainstNegativeOrZero(durationDays.Value, nameof(durationDays));
            DurationDays = durationDays.Value;
        }

        if (trialDays.HasValue)
        {
            if (trialDays.Value < 0)
                throw new DomainException("Trial days cannot be negative");
            TrialDays = trialDays;
        }

        if (features is not null)
            Features = features;

        if (isActive.HasValue)
            IsActive = isActive.Value;

        if (displayOrder.HasValue)
        {
            Guard.AgainstNegative(displayOrder.Value, nameof(displayOrder));
            DisplayOrder = displayOrder.Value;
        }

        if (billingCycle.HasValue)
            BillingCycle = billingCycle.Value;

        if (maxUsers.HasValue)
        {
            Guard.AgainstNegativeOrZero(maxUsers.Value, nameof(maxUsers));
            MaxUsers = maxUsers.Value;
        }

        if (setupFee.HasValue)
        {
            Guard.AgainstNegative(setupFee.Value, nameof(setupFee));
            _setupFee = setupFee;
        }

        if (currency is not null)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new DomainException("Currency must be a 3-letter code");
            Currency = currency.ToUpperInvariant();
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionPlanUpdatedEvent(Id, Name));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionPlanActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionPlanDeactivatedEvent(Id, Name));
    }

    public bool CanBeDeleted()
    {
        // Plan can be deleted if no active subscriptions exist
        return !UserSubscriptions.Any(us => us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial);
    }

    public void Delete()
    {
        if (!CanBeDeleted())
            throw new DomainException("Aktif abonelikleri olan plan silinemez");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionPlanDeletedEvent(Id, Name));
    }
}

