namespace Merge.Application.DTOs.Subscription;

public class SubscriptionPaymentDto
{
    public Guid Id { get; set; }
    public Guid UserSubscriptionId { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
