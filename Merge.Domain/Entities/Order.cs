using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = Enums.PaymentStatus.Pending;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public Guid? CouponId { get; set; }
    public decimal? CouponDiscount { get; set; }

    // ✅ CONCURRENCY: Eşzamanlı sipariş güncellemelerini önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public Shipping? Shipping { get; set; }
    public ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
    public Invoice? Invoice { get; set; }
    public ICollection<GiftCardTransaction> GiftCardTransactions { get; set; } = new List<GiftCardTransaction>();
    public decimal? GiftCardDiscount { get; set; }
    public Guid? ParentOrderId { get; set; } // If this order is a split from another order
    public bool IsSplitOrder { get; set; } = false;
    
    // Navigation properties for splits
    public Order? ParentOrder { get; set; }
    public ICollection<Order> SplitOrders { get; set; } = new List<Order>();
    public ICollection<OrderSplit> OriginalSplits { get; set; } = new List<OrderSplit>();
    public ICollection<OrderSplit> SplitFrom { get; set; } = new List<OrderSplit>();
}

