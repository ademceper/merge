using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class OrderSplit : BaseEntity
{
    public Guid OriginalOrderId { get; set; }
    public Guid SplitOrderId { get; set; }
    public string SplitReason { get; set; } = string.Empty; // Different shipping address, Different seller, Stock availability, etc.
    public Guid? NewAddressId { get; set; } // If split due to different address
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public OrderSplitStatus Status { get; set; } = OrderSplitStatus.Pending;

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order OriginalOrder { get; set; } = null!;
    public Order SplitOrder { get; set; } = null!;
    public Address? NewAddress { get; set; }
    public ICollection<OrderSplitItem> OrderSplitItems { get; set; } = new List<OrderSplitItem>();
}

public class OrderSplitItem : BaseEntity
{
    public Guid OrderSplitId { get; set; }
    public Guid OriginalOrderItemId { get; set; }
    public Guid SplitOrderItemId { get; set; }
    public int Quantity { get; set; } // How many items moved to split order
    
    // Navigation properties
    public OrderSplit OrderSplit { get; set; } = null!;
    public OrderItem OriginalOrderItem { get; set; } = null!;
    public OrderItem SplitOrderItem { get; set; } = null!;
}

