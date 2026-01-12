using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;

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

        return new LiveStreamOrder
        {
            Id = Guid.NewGuid(),
            LiveStreamId = liveStreamId,
            OrderId = orderId,
            ProductId = productId,
            _orderAmount = orderAmount,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

