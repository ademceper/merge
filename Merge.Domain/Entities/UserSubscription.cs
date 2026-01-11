using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using PaymentStatus = Merge.Domain.Enums.PaymentStatus;

namespace Merge.Domain.Entities;

/// <summary>
/// UserSubscription Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserSubscription : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Active;
    
    public DateTime StartDate { get; private set; } = DateTime.UtcNow;
    public DateTime EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public bool AutoRenew { get; private set; } = true;
    public DateTime? NextBillingDate { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects - CurrentPrice backing field (EF Core compatibility)
    private decimal _currentPrice;
    public decimal CurrentPrice 
    { 
        get => _currentPrice; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(CurrentPrice));
            _currentPrice = value;
        } 
    }
    
    public string? PaymentMethodId { get; private set; } // Reference to payment method
    public int RenewalCount { get; private set; } = 0; // How many times renewed

    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money CurrentPriceMoney => new Money(_currentPrice, SubscriptionPlan?.Currency ?? "TRY");

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;
    public ICollection<SubscriptionPayment> Payments { get; private set; } = new List<SubscriptionPayment>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private UserSubscription() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static UserSubscription Create(
        Guid userId,
        SubscriptionPlan plan,
        bool autoRenew = true,
        string? paymentMethodId = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNull(plan, nameof(plan));
        
        if (!plan.IsActive)
            throw new DomainException("Aktif olmayan bir plana abone olunamaz");

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(plan.DurationDays);
        DateTime? trialEndDate = null;
        SubscriptionStatus initialStatus = SubscriptionStatus.Active;

        if (plan.TrialDays.HasValue && plan.TrialDays.Value > 0)
        {
            trialEndDate = startDate.AddDays(plan.TrialDays.Value);
            initialStatus = SubscriptionStatus.Trial;
        }

        var subscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionPlanId = plan.Id,
            SubscriptionPlan = plan,
            Status = initialStatus,
            StartDate = startDate,
            EndDate = endDate,
            TrialEndDate = trialEndDate,
            AutoRenew = autoRenew,
            NextBillingDate = trialEndDate ?? endDate,
            _currentPrice = plan.Price,
            PaymentMethodId = paymentMethodId,
            RenewalCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        subscription.AddDomainEvent(new UserSubscriptionCreatedEvent(
            subscription.Id,
            userId,
            plan.Id,
            initialStatus,
            plan.Price,
            startDate,
            endDate));

        return subscription;
    }

    // ✅ BOLUM 1.1: Domain Method - Cancel subscription
    public void Cancel(string? reason = null)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Abonelik zaten iptal edilmiş");

        if (Status == SubscriptionStatus.Expired)
            throw new DomainException("Süresi dolmuş abonelik iptal edilemez");

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        AutoRenew = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new UserSubscriptionCancelledEvent(Id, UserId, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Renew subscription
    public void Renew(SubscriptionPlan plan)
    {
        Guard.AgainstNull(plan, nameof(plan));

        if (Status != SubscriptionStatus.Active)
            throw new DomainException("Sadece aktif abonelikler yenilenebilir");

        if (!plan.IsActive)
            throw new DomainException("Aktif olmayan bir plana yenilenemez");

        EndDate = EndDate.AddDays(plan.DurationDays);
        NextBillingDate = EndDate;
        RenewalCount++;
        _currentPrice = plan.Price; // Update to current plan price
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new UserSubscriptionRenewedEvent(Id, UserId, EndDate, RenewalCount));
    }

    // ✅ BOLUM 1.1: Domain Method - Suspend subscription
    public void Suspend()
    {
        if (Status != SubscriptionStatus.Active)
            throw new DomainException("Sadece aktif abonelikler askıya alınabilir");

        Status = SubscriptionStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new UserSubscriptionSuspendedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate subscription
    public void Activate()
    {
        if (Status != SubscriptionStatus.Suspended)
            throw new DomainException("Sadece askıya alınmış abonelikler aktifleştirilebilir");

        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new UserSubscriptionActivatedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Update auto-renew setting
    public void UpdateAutoRenew(bool autoRenew)
    {
        if (AutoRenew == autoRenew)
            return;

        AutoRenew = autoRenew;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new UserSubscriptionUpdatedEvent(Id, UserId, AutoRenewChanged: true, PaymentMethodChanged: null));
    }

    // ✅ BOLUM 1.1: Domain Method - Update payment method
    public void UpdatePaymentMethod(string? paymentMethodId)
    {
        var paymentMethodChanged = PaymentMethodId != paymentMethodId;
        PaymentMethodId = paymentMethodId;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        if (paymentMethodChanged)
        {
            AddDomainEvent(new UserSubscriptionUpdatedEvent(Id, UserId, AutoRenewChanged: null, PaymentMethodChanged: true));
        }
    }

    // ✅ BOLUM 1.1: Domain Method - Check if subscription is active
    public bool IsActive()
    {
        return Status == SubscriptionStatus.Active && EndDate > DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Check if subscription is in trial
    public bool IsInTrial()
    {
        return Status == SubscriptionStatus.Trial && 
               TrialEndDate.HasValue && 
               TrialEndDate.Value > DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Get days remaining
    public int GetDaysRemaining()
    {
        if (EndDate <= DateTime.UtcNow)
            return 0;

        return (int)(EndDate - DateTime.UtcNow).TotalDays;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as expired
    public void MarkAsExpired()
    {
        if (Status == SubscriptionStatus.Expired)
            return;

        if (EndDate <= DateTime.UtcNow && Status != SubscriptionStatus.Cancelled)
        {
            Status = SubscriptionStatus.Expired;
            AutoRenew = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

