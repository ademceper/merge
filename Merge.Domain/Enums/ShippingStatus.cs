namespace Merge.Domain.Enums;

/// <summary>
/// Shipping status values for Shipping entity
/// </summary>
public enum ShippingStatus
{
    Preparing = 0,
    Shipped = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Returned = 5,
    Failed = 6
}
