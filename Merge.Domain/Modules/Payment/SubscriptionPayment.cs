using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using PaymentStatus = Merge.Domain.Enums.PaymentStatus;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// SubscriptionPayment Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionPayment : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserSubscriptionId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    
    // ✅ BOLUM 1.3: Value Objects - Amount backing field (EF Core compatibility)
    private decimal _amount;
    public decimal Amount 
    { 
        get => _amount; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Amount));
            _amount = value;
        } 
    }
    
    public string? TransactionId { get; private set; } // External payment gateway transaction ID
    public DateTime? PaidAt { get; private set; }
    public DateTime BillingPeriodStart { get; private set; }
    public DateTime BillingPeriodEnd { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; } = 0;
    public DateTime? NextRetryDate { get; private set; }
    
    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money AmountMoney => new Money(_amount, UserSubscription?.SubscriptionPlan?.Currency ?? "TRY");

    // Navigation properties
    public UserSubscription UserSubscription { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SubscriptionPayment() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SubscriptionPayment Create(
        UserSubscription subscription,
        decimal amount,
        DateTime billingPeriodStart,
        DateTime billingPeriodEnd)
    {
        Guard.AgainstNull(subscription, nameof(subscription));
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        if (billingPeriodStart >= billingPeriodEnd)
            throw new DomainException("Billing period start date must be before end date");

        var payment = new SubscriptionPayment
        {
            Id = Guid.NewGuid(),
            UserSubscriptionId = subscription.Id,
            UserSubscription = subscription,
            PaymentStatus = PaymentStatus.Pending,
            _amount = amount,
            BillingPeriodStart = billingPeriodStart,
            BillingPeriodEnd = billingPeriodEnd,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        return payment;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark payment as completed
    public void MarkAsCompleted(string transactionId)
    {
        Guard.AgainstNullOrEmpty(transactionId, nameof(transactionId));

        if (PaymentStatus == PaymentStatus.Completed)
            throw new DomainException("Ödeme zaten tamamlanmış");

        PaymentStatus = PaymentStatus.Completed;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
        FailureReason = null;
        NextRetryDate = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        // ✅ ARCHITECTURE: Payment event'leri SubscriptionPayment üzerinden dispatch edilir
        AddDomainEvent(new SubscriptionPaymentCompletedEvent(Id, UserSubscriptionId, _amount, transactionId));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark payment as failed
    public void MarkAsFailed(string reason)
    {
        Guard.AgainstNullOrEmpty(reason, nameof(reason));

        if (PaymentStatus == PaymentStatus.Completed)
            throw new DomainException("Tamamlanmış ödeme başarısız olarak işaretlenemez");

        PaymentStatus = PaymentStatus.Failed;
        FailureReason = reason;
        RetryCount++;
        NextRetryDate = DateTime.UtcNow.AddDays(1); // Retry next day
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        // ✅ ARCHITECTURE: Payment event'leri SubscriptionPayment üzerinden dispatch edilir
        AddDomainEvent(new SubscriptionPaymentFailedEvent(Id, UserSubscriptionId, _amount, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Retry payment
    public void Retry()
    {
        if (PaymentStatus != PaymentStatus.Failed)
            throw new DomainException("Sadece başarısız ödemeler tekrar denenebilir");

        PaymentStatus = PaymentStatus.Pending;
        RetryCount++;
        NextRetryDate = null;
        FailureReason = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        // ✅ ARCHITECTURE: Payment event'leri SubscriptionPayment üzerinden dispatch edilir
        AddDomainEvent(new SubscriptionPaymentRetriedEvent(Id, UserSubscriptionId, RetryCount));
    }

    // ✅ BOLUM 1.1: Domain Method - Check if payment can be retried
    public bool CanRetry(int maxRetries = 3)
    {
        return PaymentStatus == PaymentStatus.Failed && RetryCount < maxRetries;
    }
}

