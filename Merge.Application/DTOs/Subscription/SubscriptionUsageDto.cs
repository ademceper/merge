namespace Merge.Application.DTOs.Subscription;

public class SubscriptionUsageDto
{
    public Guid Id { get; set; }
    public Guid UserSubscriptionId { get; set; }
    public string Feature { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int? Limit { get; set; }
    public int? Remaining { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
