using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// EmailCampaign Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailCampaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string ReplyToEmail { get; set; } = string.Empty;
    public Guid? TemplateId { get; set; }
    public EmailTemplate? Template { get; set; }
    public string Content { get; set; } = string.Empty; // HTML content
    public EmailCampaignStatus Status { get; set; } = EmailCampaignStatus.Draft;
    public EmailCampaignType Type { get; set; } = EmailCampaignType.Promotional;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string TargetSegment { get; set; } = string.Empty; // All, Active, Inactive, Buyers, etc.
    public int TotalRecipients { get; set; } = 0;
    public int SentCount { get; set; } = 0;
    public int DeliveredCount { get; set; } = 0;
    public int OpenedCount { get; set; } = 0;
    public int ClickedCount { get; set; } = 0;
    public int BouncedCount { get; set; } = 0;
    public int UnsubscribedCount { get; set; } = 0;
    public decimal OpenRate { get; set; } = 0;
    public decimal ClickRate { get; set; } = 0;
    public string? Tags { get; set; } // JSON array of tags
    public ICollection<EmailCampaignRecipient> Recipients { get; set; } = new List<EmailCampaignRecipient>();
}
