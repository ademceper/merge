namespace Merge.Domain.Enums;

/// <summary>
/// Transaction status for SellerTransaction entity
/// </summary>
public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3,
    Reversed = 4
}
