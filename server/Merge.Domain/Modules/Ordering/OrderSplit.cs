using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// OrderSplit Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OrderSplit : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OriginalOrderId { get; private set; }
    public Guid SplitOrderId { get; private set; }
    public string SplitReason { get; private set; } = string.Empty; // Different shipping address, Different seller, Stock availability, etc.
    public Guid? NewAddressId { get; private set; } // If split due to different address
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public OrderSplitStatus Status { get; private set; } = OrderSplitStatus.Pending;

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.7: Concurrency Control (ZORUNLU)
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

    // ✅ BOLUM 1.1: Domain Logic - Mark as processing
    public void MarkAsProcessing()
    {
        if (Status != OrderSplitStatus.Pending)
            throw new DomainException("Sadece bekleyen sipariş bölünmeleri işleme alınabilir");

        Status = OrderSplitStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Event - Order Split Processing
        AddDomainEvent(new OrderSplitProcessingEvent(Id, OriginalOrderId, SplitOrderId));
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

