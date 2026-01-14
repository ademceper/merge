using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Marketplace;
namespace Merge.Domain.Enums;

/// <summary>
/// Transaction status for SellerTransaction, ETLProcess entities
/// </summary>
public enum TransactionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Reversed = 5
}
