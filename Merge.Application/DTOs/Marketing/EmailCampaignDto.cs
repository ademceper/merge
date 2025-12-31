namespace Merge.Application.DTOs.Marketing;

public class EmailCampaignDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string TargetSegment { get; set; } = string.Empty;
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public DateTime CreatedAt { get; set; }
}
