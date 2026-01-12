using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// OrderSplitItem Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class OrderSplitItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderSplitId { get; private set; }
    public Guid OriginalOrderItemId { get; private set; }
    public Guid SplitOrderItemId { get; private set; }
    public int Quantity { get; private set; } // How many items moved to split order
    
    // Navigation properties
    public OrderSplit OrderSplit { get; private set; } = null!;
    public OrderItem OriginalOrderItem { get; private set; } = null!;
    public OrderItem SplitOrderItem { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private OrderSplitItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static OrderSplitItem Create(
        Guid orderSplitId,
        Guid originalOrderItemId,
        Guid splitOrderItemId,
        int quantity,
        OrderSplit orderSplit,
        OrderItem originalOrderItem,
        OrderItem splitOrderItem)
    {
        Guard.AgainstDefault(orderSplitId, nameof(orderSplitId));
        Guard.AgainstDefault(originalOrderItemId, nameof(originalOrderItemId));
        Guard.AgainstDefault(splitOrderItemId, nameof(splitOrderItemId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNull(orderSplit, nameof(orderSplit));
        Guard.AgainstNull(originalOrderItem, nameof(originalOrderItem));
        Guard.AgainstNull(splitOrderItem, nameof(splitOrderItem));

        return new OrderSplitItem
        {
            Id = Guid.NewGuid(),
            OrderSplitId = orderSplitId,
            OriginalOrderItemId = originalOrderItemId,
            SplitOrderItemId = splitOrderItemId,
            Quantity = quantity,
            OrderSplit = orderSplit,
            OriginalOrderItem = originalOrderItem,
            SplitOrderItem = splitOrderItem,
            CreatedAt = DateTime.UtcNow
        };
    }
}

