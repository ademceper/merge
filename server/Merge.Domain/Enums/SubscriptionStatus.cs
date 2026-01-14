using Merge.Domain.Modules.Payment;
namespace Merge.Domain.Enums;

/// <summary>
/// Subscription status values for UserSubscription entity
/// </summary>
public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    Suspended = 2,
    Cancelled = 3,
    Expired = 4,
    PastDue = 5
}
