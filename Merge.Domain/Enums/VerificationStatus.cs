namespace Merge.Domain.Enums;

/// <summary>
/// Verification status for OrderVerification, SecurityMonitoring entities
/// </summary>
public enum VerificationStatus
{
    Pending = 0,
    Verified = 1,
    Failed = 2,
    Rejected = 3,
    Expired = 4
}
