namespace Merge.Application.DTOs.Security;

public class PaymentFraudPreventionDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string PaymentTransactionId { get; set; } = string.Empty;
    public string CheckType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public int RiskScore { get; set; }
    public Dictionary<string, object>? CheckResult { get; set; }
    public DateTime? CheckedAt { get; set; }
    public string? DeviceFingerprint { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
