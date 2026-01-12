using Merge.Domain.Modules.Ordering;
namespace Merge.Domain.Enums;

/// <summary>
/// Order split status values for OrderSplit entity
/// </summary>
public enum OrderSplitStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3
}
