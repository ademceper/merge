using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerTransaction Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerTransaction : BaseEntity, IAggregateRoot
{
    public Guid SellerId { get; private set; }
    public SellerTransactionType TransactionType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public Guid? RelatedEntityId { get; private set; } // CommissionId, PayoutId, OrderId
    public string? RelatedEntityType { get; private set; }
    public FinanceTransactionStatus Status { get; private set; } = FinanceTransactionStatus.Pending;

    // Navigation properties
    public User Seller { get; private set; } = null!;

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    private SellerTransaction() { }

    public static SellerTransaction Create(
        Guid sellerId,
        SellerTransactionType transactionType,
        string description,
        decimal amount,
        decimal balanceBefore,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstNullOrEmpty(description, nameof(description));

        var balanceAfter = balanceBefore + amount;

        var transaction = new SellerTransaction
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            TransactionType = transactionType,
            Description = description,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            Status = FinanceTransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        transaction.AddDomainEvent(new SellerTransactionCreatedEvent(transaction.Id, sellerId, transactionType.ToString(), amount));

        return transaction;
    }

    public void Complete()
    {
        if (Status == FinanceTransactionStatus.Completed)
            throw new DomainException("İşlem zaten tamamlanmış");

        if (Status == FinanceTransactionStatus.Cancelled)
            throw new DomainException("İptal edilmiş işlem tamamlanamaz");

        Status = FinanceTransactionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerTransactionCompletedEvent(Id, SellerId, Amount));
    }

    public void Fail()
    {
        if (Status == FinanceTransactionStatus.Completed)
            throw new DomainException("Tamamlanmış işlem başarısız olarak işaretlenemez");

        if (Status == FinanceTransactionStatus.Failed)
            throw new DomainException("İşlem zaten başarısız olarak işaretlenmiş");

        Status = FinanceTransactionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerTransactionFailedEvent(Id, SellerId, Amount));
    }

    public void Cancel()
    {
        if (Status == FinanceTransactionStatus.Completed)
            throw new DomainException("Tamamlanmış işlem iptal edilemez");

        if (Status == FinanceTransactionStatus.Cancelled)
            throw new DomainException("İşlem zaten iptal edilmiş");

        Status = FinanceTransactionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerTransactionCancelledEvent(Id, SellerId, Amount));
    }

    public bool IsCompleted() => Status == FinanceTransactionStatus.Completed;
}

