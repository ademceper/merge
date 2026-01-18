using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// LiveStreamOrder Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStreamOrder : BaseEntity, IAggregateRoot
{
    public Guid LiveStreamId { get; private set; }
    public LiveStream LiveStream { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid? ProductId { get; private set; } // Product that triggered the order
    public Product? Product { get; private set; }
    
    private decimal _orderAmount;
    public decimal OrderAmount 
    { 
        get => _orderAmount; 
        private set 
        { 
            Guard.AgainstNegativeOrZero(value, nameof(OrderAmount));
            _orderAmount = value;
        } 
    }
    
    [NotMapped]
    public Money OrderAmountMoney => new Money(_orderAmount);

    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private LiveStreamOrder() { }

    public static LiveStreamOrder Create(
        Guid liveStreamId,
        Guid orderId,
        Money orderAmount,
        Guid? productId = null)
    {
        Guard.AgainstDefault(liveStreamId, nameof(liveStreamId));
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNull(orderAmount, nameof(orderAmount));
        Guard.AgainstNegativeOrZero(orderAmount.Amount, nameof(orderAmount));

        var streamOrder = new LiveStreamOrder
        {
            Id = Guid.NewGuid(),
            LiveStreamId = liveStreamId,
            OrderId = orderId,
            ProductId = productId,
            _orderAmount = orderAmount.Amount,
            CreatedAt = DateTime.UtcNow
        };

        streamOrder.ValidateInvariants();

        streamOrder.AddDomainEvent(new LiveStreamOrderCreatedEvent(
            liveStreamId,
            orderId,
            productId,
            orderAmount.Amount,
            streamOrder.CreatedAt));

        return streamOrder;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamOrderDeletedEvent(LiveStreamId, OrderId, OrderAmount, UpdatedAt.Value));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamOrderRestoredEvent(LiveStreamId, OrderId, OrderAmount, UpdatedAt.Value));
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == LiveStreamId)
            throw new DomainException("Canlı yayın ID boş olamaz");

        if (Guid.Empty == OrderId)
            throw new DomainException("Sipariş ID boş olamaz");

        if (OrderAmount <= 0)
            throw new DomainException("Sipariş tutarı pozitif olmalıdır");
    }
}

