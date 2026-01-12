using Merge.Domain.Modules.Ordering;
namespace Merge.Domain.Enums;

/// <summary>
/// Order status values for Order entity
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4,
    Refunded = 5,
    OnHold = 6
}
