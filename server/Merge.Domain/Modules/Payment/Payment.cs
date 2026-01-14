using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Ordering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// Payment Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Payment : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string PaymentProvider { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Money
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
    
    public string? TransactionId { get; private set; }
    public string? PaymentReference { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? Metadata { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.3: Value Object property
    [NotMapped]
    public Money AmountMoney => new Money(_amount);

    // Navigation properties
    public Order Order { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Payment() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Payment Create(
        Guid orderId,
        string paymentMethod,
        string paymentProvider,
        Money amount)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNullOrEmpty(paymentMethod, nameof(paymentMethod));
        Guard.AgainstNullOrEmpty(paymentProvider, nameof(paymentProvider));
        Guard.AgainstNull(amount, nameof(amount));

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            PaymentMethod = paymentMethod,
            PaymentProvider = paymentProvider,
            _amount = amount.Amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - PaymentCreatedEvent yayınla
        payment.AddDomainEvent(new PaymentCreatedEvent(
            payment.Id,
            orderId,
            paymentMethod,
            paymentProvider,
            amount.Amount));

        return payment;
    }

    // ✅ BOLUM 1.1: State Machine Pattern - Transition to new status
    private static readonly Dictionary<PaymentStatus, PaymentStatus[]> AllowedTransitions = new()
    {
        { PaymentStatus.Pending, new[] { PaymentStatus.Processing, PaymentStatus.Cancelled } },
        { PaymentStatus.Processing, new[] { PaymentStatus.Completed, PaymentStatus.Failed } },
        { PaymentStatus.Completed, new[] { PaymentStatus.Refunded, PaymentStatus.PartiallyRefunded } },
        { PaymentStatus.Failed, Array.Empty<PaymentStatus>() }, // Terminal state
        { PaymentStatus.Cancelled, Array.Empty<PaymentStatus>() }, // Terminal state
        { PaymentStatus.Refunded, Array.Empty<PaymentStatus>() }, // Terminal state
        { PaymentStatus.PartiallyRefunded, new[] { PaymentStatus.Refunded } }
    };

    public void TransitionTo(PaymentStatus newStatus)
    {
        if (!AllowedTransitions.ContainsKey(Status))
            throw new InvalidStateTransitionException(Status, newStatus);

        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new InvalidStateTransitionException(Status, newStatus);

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == PaymentStatus.Completed)
            PaidAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Convenience methods for common transitions
    public void Process()
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException("Sadece bekleyen ödemeler işleme alınabilir");

        TransitionTo(PaymentStatus.Processing);

        // ✅ BOLUM 1.5: Domain Events - PaymentProcessingEvent yayınla
        AddDomainEvent(new PaymentProcessingEvent(
            Id,
            OrderId,
            PaymentMethod,
            PaymentProvider,
            _amount));
    }

    public void Complete(string transactionId, string? paymentReference = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new DomainException("Sadece işlenmekte olan ödemeler tamamlanabilir");

        Guard.AgainstNullOrEmpty(transactionId, nameof(transactionId));

        TransactionId = transactionId;
        PaymentReference = paymentReference;
        TransitionTo(PaymentStatus.Completed);

        // ✅ BOLUM 1.5: Domain Events - PaymentCompletedEvent yayınla
        AddDomainEvent(new PaymentCompletedEvent(
            Id,
            OrderId,
            transactionId,
            paymentReference,
            _amount));
    }

    public void Fail(string failureReason)
    {
        if (Status != PaymentStatus.Processing)
            throw new DomainException("Sadece işlenmekte olan ödemeler başarısız olarak işaretlenebilir");

        Guard.AgainstNullOrEmpty(failureReason, nameof(failureReason));

        FailureReason = failureReason;
        TransitionTo(PaymentStatus.Failed);

        // ✅ BOLUM 1.5: Domain Events - PaymentFailedEvent yayınla
        AddDomainEvent(new PaymentFailedEvent(
            Id,
            OrderId,
            failureReason));
    }

    public void Cancel(string? reason = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException("Sadece bekleyen ödemeler iptal edilebilir");

        if (!string.IsNullOrEmpty(reason))
            FailureReason = reason;

        TransitionTo(PaymentStatus.Cancelled);

        // ✅ BOLUM 1.5: Domain Events - PaymentCancelledEvent yayınla
        AddDomainEvent(new PaymentCancelledEvent(
            Id,
            OrderId,
            reason));
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
            throw new DomainException("Sadece tamamlanmış ödemeler iade edilebilir");

        TransitionTo(PaymentStatus.Refunded);

        // ✅ BOLUM 1.5: Domain Events - PaymentRefundedEvent yayınla
        AddDomainEvent(new PaymentRefundedEvent(
            Id,
            OrderId,
            _amount,
            IsFullRefund: true));
    }

    public void PartiallyRefund(Money refundAmount)
    {
        Guard.AgainstNull(refundAmount, nameof(refundAmount));

        if (Status != PaymentStatus.Completed)
            throw new DomainException("Sadece tamamlanmış ödemeler kısmen iade edilebilir");

        if (refundAmount.Amount >= _amount)
            throw new DomainException("Kısmi iade tutarı toplam tutardan küçük olmalıdır");

        TransitionTo(PaymentStatus.PartiallyRefunded);

        // ✅ BOLUM 1.5: Domain Events - PaymentRefundedEvent yayınla
        AddDomainEvent(new PaymentRefundedEvent(
            Id,
            OrderId,
            refundAmount.Amount,
            IsFullRefund: false));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set transaction ID
    public void SetTransactionId(string transactionId)
    {
        Guard.AgainstNullOrEmpty(transactionId, nameof(transactionId));
        TransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PaymentUpdatedEvent yayınla
        AddDomainEvent(new PaymentUpdatedEvent(Id, OrderId, transactionId, PaymentReference, Metadata));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set payment reference
    public void SetPaymentReference(string paymentReference)
    {
        Guard.AgainstNullOrEmpty(paymentReference, nameof(paymentReference));
        PaymentReference = paymentReference;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PaymentUpdatedEvent yayınla
        AddDomainEvent(new PaymentUpdatedEvent(Id, OrderId, TransactionId, paymentReference, Metadata));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set metadata
    public void SetMetadata(string metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PaymentUpdatedEvent yayınla
        AddDomainEvent(new PaymentUpdatedEvent(Id, OrderId, TransactionId, PaymentReference, metadata));
    }
}

