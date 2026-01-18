using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using PaymentStatus = Merge.Domain.Enums.PaymentStatus;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// UserSubscription Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserSubscription : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Active;
    
    public DateTime StartDate { get; private set; } = DateTime.UtcNow;
    public DateTime EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public bool AutoRenew { get; private set; } = true;
    public DateTime? NextBillingDate { get; private set; }
    
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

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money CurrentPriceMoney => new Money(_currentPrice, SubscriptionPlan?.Currency ?? "TRY");

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;
    public ICollection<SubscriptionPayment> Payments { get; private set; } = [];

    private UserSubscription() { }

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

        AddDomainEvent(new UserSubscriptionCancelledEvent(Id, UserId, reason));
    }

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

        AddDomainEvent(new UserSubscriptionRenewedEvent(Id, UserId, EndDate, RenewalCount));
    }

    public void Suspend()
    {
        if (Status != SubscriptionStatus.Active)
            throw new DomainException("Sadece aktif abonelikler askıya alınabilir");

        Status = SubscriptionStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserSubscriptionSuspendedEvent(Id, UserId));
    }

    public void Activate()
    {
        if (Status != SubscriptionStatus.Suspended)
            throw new DomainException("Sadece askıya alınmış abonelikler aktifleştirilebilir");

        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserSubscriptionActivatedEvent(Id, UserId));
    }

    public void UpdateAutoRenew(bool autoRenew)
    {
        if (AutoRenew == autoRenew)
            return;

        AutoRenew = autoRenew;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserSubscriptionUpdatedEvent(Id, UserId, AutoRenewChanged: true, PaymentMethodChanged: null));
    }

    public void UpdatePaymentMethod(string? paymentMethodId)
    {
        var paymentMethodChanged = PaymentMethodId != paymentMethodId;
        PaymentMethodId = paymentMethodId;
        UpdatedAt = DateTime.UtcNow;

        if (paymentMethodChanged)
        {
            AddDomainEvent(new UserSubscriptionUpdatedEvent(Id, UserId, AutoRenewChanged: null, PaymentMethodChanged: true));
        }
    }

    public bool IsActive()
    {
        return Status == SubscriptionStatus.Active && EndDate > DateTime.UtcNow;
    }

    public bool IsInTrial()
    {
        return Status == SubscriptionStatus.Trial && 
               TrialEndDate.HasValue && 
               TrialEndDate.Value > DateTime.UtcNow;
    }

    public int GetDaysRemaining()
    {
        if (EndDate <= DateTime.UtcNow)
            return 0;

        return (int)(EndDate - DateTime.UtcNow).TotalDays;
    }

    public void MarkAsExpired()
    {
        if (Status == SubscriptionStatus.Expired)
            return;

        if (EndDate <= DateTime.UtcNow && Status != SubscriptionStatus.Cancelled)
        {
            Status = SubscriptionStatus.Expired;
            AutoRenew = false;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserSubscriptionExpiredEvent(Id, UserId));
        }
    }
}

