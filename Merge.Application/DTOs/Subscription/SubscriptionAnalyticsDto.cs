namespace Merge.Application.DTOs.Subscription;

public class SubscriptionAnalyticsDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal AnnualRecurringRevenue { get; set; }
    public decimal ChurnRate { get; set; } // Percentage
    public decimal AverageRevenuePerUser { get; set; }
    public Dictionary<string, int> SubscriptionsByPlan { get; set; } = new();
    public Dictionary<string, decimal> RevenueByPlan { get; set; } = new();
    public List<SubscriptionTrendDto> Trends { get; set; } = new();
}
