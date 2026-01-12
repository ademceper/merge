using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// OrderSplit Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class OrderSplit : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OriginalOrderId { get; private set; }
    public Guid SplitOrderId { get; private set; }
    public string SplitReason { get; private set; } = string.Empty; // Different shipping address, Different seller, Stock availability, etc.
    public Guid? NewAddressId { get; private set; } // If split due to different address
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public OrderSplitStatus Status { get; private set; } = OrderSplitStatus.Pending;

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order OriginalOrder { get; private set; } = null!;
    public Order SplitOrder { get; private set; } = null!;
    public Address? NewAddress { get; private set; }
    public ICollection<OrderSplitItem> OrderSplitItems { get; private set; } = new List<OrderSplitItem>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private OrderSplit() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static OrderSplit Create(
        Guid originalOrderId,
        Guid splitOrderId,
        string splitReason,
        Guid? newAddressId,
        Order originalOrder,
        Order splitOrder,
        Address? newAddress = null)
    {
        Guard.AgainstDefault(originalOrderId, nameof(originalOrderId));
        Guard.AgainstDefault(splitOrderId, nameof(splitOrderId));
        Guard.AgainstNullOrEmpty(splitReason, nameof(splitReason));
        Guard.AgainstNull(originalOrder, nameof(originalOrder));
        Guard.AgainstNull(splitOrder, nameof(splitOrder));

        var orderSplit = new OrderSplit
        {
            Id = Guid.NewGuid(),
            OriginalOrderId = originalOrderId,
            SplitOrderId = splitOrderId,
            SplitReason = splitReason,
            NewAddressId = newAddressId,
            Status = OrderSplitStatus.Pending,
            OriginalOrder = originalOrder,
            SplitOrder = splitOrder,
            NewAddress = newAddress,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Event - Order Split Created
        orderSplit.AddDomainEvent(new OrderSplitCreatedEvent(
            orderSplit.Id,
            originalOrderId,
            splitOrderId,
            splitReason));

        return orderSplit;
    }

    // ✅ BOLUM 1.1: Domain Logic - Cancel order split
    public void Cancel()
    {
        if (Status == OrderSplitStatus.Completed)
            throw new DomainException("Tamamlanmış sipariş bölünmesi iptal edilemez");

        if (Status == OrderSplitStatus.Cancelled)
            throw new DomainException("Sipariş bölünmesi zaten iptal edilmiş");

        Status = OrderSplitStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Order Split Cancelled
        AddDomainEvent(new OrderSplitCancelledEvent(Id, OriginalOrderId, SplitOrderId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Complete order split
    public void Complete()
    {
        if (Status != OrderSplitStatus.Pending && Status != OrderSplitStatus.Processing)
            throw new DomainException("Sadece bekleyen veya işlenmekte olan sipariş bölünmeleri tamamlanabilir");

        Status = OrderSplitStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Order Split Completed
        AddDomainEvent(new OrderSplitCompletedEvent(Id, OriginalOrderId, SplitOrderId));
    }
}

