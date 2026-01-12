using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
namespace Merge.Domain.Enums;

/// <summary>
/// Alert status for SecurityAlert, FraudAlert entities
/// </summary>
public enum AlertStatus
{
    New = 0,
    Acknowledged = 1,
    Investigating = 2,
    Resolved = 3,
    Dismissed = 4,
    FalsePositive = 5
}
