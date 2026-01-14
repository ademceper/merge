using Merge.Domain.Modules.Ordering;
namespace Merge.Domain.Enums;

/// <summary>
/// Return request status values for ReturnRequest entity
/// </summary>
public enum ReturnRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Processing = 3,
    Completed = 4,
    Cancelled = 5
}
