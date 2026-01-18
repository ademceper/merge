using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// CommissionPayout Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class CommissionPayout : BaseEntity, IAggregateRoot
{
    public Guid SellerId { get; private set; }
    public string PayoutNumber { get; private set; } = string.Empty; // Auto-generated: PAY-XXXXXX
    public decimal TotalAmount { get; private set; }
    public decimal TransactionFee { get; private set; } = 0;
    public decimal NetAmount { get; private set; }
    public PayoutStatus Status { get; private set; } = PayoutStatus.Pending;
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? PaymentDetails { get; private set; }
    public string? TransactionReference { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public User Seller { get; private set; } = null!;
    private readonly List<CommissionPayoutItem> _items = new();
    public IReadOnlyCollection<CommissionPayoutItem> Items => _items.AsReadOnly();

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    private CommissionPayout() { }

    public static CommissionPayout Create(
        Guid sellerId,
        string payoutNumber,
        decimal totalAmount,
        decimal transactionFee,
        string paymentMethod,
        string? paymentDetails = null)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstNullOrEmpty(payoutNumber, nameof(payoutNumber));
        Guard.AgainstNegativeOrZero(totalAmount, nameof(totalAmount));
        Guard.AgainstNegative(transactionFee, nameof(transactionFee));
        Guard.AgainstNullOrEmpty(paymentMethod, nameof(paymentMethod));

        var netAmount = totalAmount - transactionFee;

        if (netAmount <= 0)
            throw new DomainException("Net ödeme tutarı sıfırdan büyük olmalıdır");

        var payout = new CommissionPayout
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            PayoutNumber = payoutNumber,
            TotalAmount = totalAmount,
            TransactionFee = transactionFee,
            NetAmount = netAmount,
            PaymentMethod = paymentMethod,
            PaymentDetails = paymentDetails,
            Status = PayoutStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        payout.AddDomainEvent(new CommissionPayoutCreatedEvent(payout.Id, sellerId, netAmount));

        return payout;
    }

    public void AddItem(Guid commissionId)
    {
        Guard.AgainstDefault(commissionId, nameof(commissionId));

        if (Status != PayoutStatus.Pending)
            throw new DomainException("Sadece bekleyen ödemelere item eklenebilir");

        if (_items.Any(i => i.CommissionId == commissionId))
            throw new DomainException("Bu komisyon zaten ödeme listesinde");

        var item = CommissionPayoutItem.Create(Id, commissionId);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Process(string transactionReference)
    {
        Guard.AgainstNullOrEmpty(transactionReference, nameof(transactionReference));

        if (Status != PayoutStatus.Pending)
            throw new DomainException("Sadece bekleyen ödemeler işleme alınabilir");

        Status = PayoutStatus.Processing;
        TransactionReference = transactionReference;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CommissionPayoutProcessedEvent(Id, SellerId, NetAmount, transactionReference));
    }

    public void Complete()
    {
        if (Status != PayoutStatus.Processing)
            throw new DomainException("Sadece işleme alınmış ödemeler tamamlanabilir");

        Status = PayoutStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CommissionPayoutCompletedEvent(Id, SellerId, NetAmount));
    }

    public void Fail(string? reason = null)
    {
        if (Status == PayoutStatus.Completed)
            throw new DomainException("Tamamlanmış ödeme başarısız olarak işaretlenemez");

        if (Status == PayoutStatus.Failed)
            throw new DomainException("Ödeme zaten başarısız olarak işaretlenmiş");

        Status = PayoutStatus.Failed;
        Notes = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CommissionPayoutFailedEvent(Id, SellerId, NetAmount, reason));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == PayoutStatus.Completed)
            throw new DomainException("Tamamlanmış ödeme iptal edilemez");

        if (Status == PayoutStatus.Cancelled)
            throw new DomainException("Ödeme zaten iptal edilmiş");

        Status = PayoutStatus.Cancelled;
        Notes = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CommissionPayoutCancelledEvent(Id, SellerId, NetAmount, reason));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeProcessed() => Status == PayoutStatus.Pending;
}

