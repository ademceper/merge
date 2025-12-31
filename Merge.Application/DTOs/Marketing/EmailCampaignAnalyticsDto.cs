namespace Merge.Application.DTOs.Marketing;

public class EmailCampaignAnalyticsDto
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public int BouncedCount { get; set; }
    public int UnsubscribedCount { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal BounceRate { get; set; }
    public decimal UnsubscribeRate { get; set; }
    public DateTime? SentAt { get; set; }
}
