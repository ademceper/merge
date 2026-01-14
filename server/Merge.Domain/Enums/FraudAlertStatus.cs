namespace Merge.Domain.Enums;

/// <summary>
/// Fraud alert status values
/// </summary>
public enum FraudAlertStatus
{
    Pending = 0,
    Reviewed = 1,
    Resolved = 2,
    FalsePositive = 3
}
