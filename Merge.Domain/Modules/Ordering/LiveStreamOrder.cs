using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// LiveStreamOrder Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStreamOrder : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid LiveStreamId { get; private set; }
    public LiveStream LiveStream { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid? ProductId { get; private set; } // Product that triggered the order
    public Product? Product { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Money (EF Core compatibility için decimal backing)
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
    
    // ✅ BOLUM 1.3: Value Object property (computed from decimal)
    [NotMapped]
    public Money OrderAmountMoney => new Money(_orderAmount);

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LiveStreamOrder() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LiveStreamOrder Create(
        Guid liveStreamId,
        Guid orderId,
        decimal orderAmount,
        Guid? productId = null)
    {
        Guard.AgainstDefault(liveStreamId, nameof(liveStreamId));
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNegativeOrZero(orderAmount, nameof(orderAmount));

        var streamOrder = new LiveStreamOrder
        {
            Id = Guid.NewGuid(),
            LiveStreamId = liveStreamId,
            OrderId = orderId,
            ProductId = productId,
            _orderAmount = orderAmount,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.4: Invariant validation
        streamOrder.ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - LiveStreamOrderCreatedEvent
        streamOrder.AddDomainEvent(new LiveStreamOrderCreatedEvent(
            liveStreamId,
            orderId,
            productId,
            orderAmount,
            streamOrder.CreatedAt));

        return streamOrder;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - LiveStreamOrderDeletedEvent
        AddDomainEvent(new LiveStreamOrderDeletedEvent(LiveStreamId, OrderId, OrderAmount, UpdatedAt.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Restore deleted order
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - LiveStreamOrderRestoredEvent
        AddDomainEvent(new LiveStreamOrderRestoredEvent(LiveStreamId, OrderId, OrderAmount, UpdatedAt.Value));
    }

    // ✅ BOLUM 1.4: Invariant validation
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

