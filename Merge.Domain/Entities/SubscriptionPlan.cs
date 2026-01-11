using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// SubscriptionPlan Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionPlan : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
    public SubscriptionPlanType PlanType { get; private set; } = SubscriptionPlanType.Monthly;
    
    // ✅ BOLUM 1.3: Value Objects - Money backing field (EF Core compatibility)
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
    
    // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
    public BillingCycle BillingCycle { get; private set; } = BillingCycle.Monthly;
    public int MaxUsers { get; private set; } = 1; // Maximum users allowed
    
    // ✅ BOLUM 1.3: Value Objects - SetupFee backing field
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

    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money PriceMoney => new Money(_price, Currency);
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? SetupFeeMoney => _setupFee.HasValue ? new Money(_setupFee.Value, Currency) : null;

    // Navigation properties
    public ICollection<UserSubscription> UserSubscriptions { get; private set; } = new List<UserSubscription>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SubscriptionPlan() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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
            throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));
        
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code", nameof(currency));

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

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        plan.AddDomainEvent(new SubscriptionPlanCreatedEvent(plan.Id, name, planType, price, currency));

        return plan;
    }

    // ✅ BOLUM 1.1: Domain Method - Update plan
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
        if (name != null)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstLength(name, 100, nameof(name));
            Name = name;
        }

        if (description != null)
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
                throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));
            TrialDays = trialDays;
        }

        if (features != null)
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

        if (currency != null)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-letter code", nameof(currency));
            Currency = currency.ToUpperInvariant();
        }

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new SubscriptionPlanUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate plan
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new SubscriptionPlanActivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate plan
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new SubscriptionPlanDeactivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Check if plan can be deleted
    public bool CanBeDeleted()
    {
        // Plan can be deleted if no active subscriptions exist
        return !UserSubscriptions.Any(us => us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial);
    }

    // ✅ BOLUM 1.1: Domain Method - Delete plan
    public void Delete()
    {
        if (!CanBeDeleted())
            throw new DomainException("Aktif abonelikleri olan plan silinemez");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new SubscriptionPlanDeletedEvent(Id, Name));
    }
}

