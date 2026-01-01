namespace Merge.Domain.Enums;

/// <summary>
/// Purchase Order status values
/// </summary>
public enum PurchaseOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Fulfilled = 4,
    Cancelled = 5
}
