namespace Merge.Application.DTOs.Subscription;

public class SubscriptionTrendDto
{
    public DateTime Date { get; set; }
    public int NewSubscriptions { get; set; }
    public int Cancellations { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal Revenue { get; set; }
}
