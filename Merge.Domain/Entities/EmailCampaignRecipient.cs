using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// EmailCampaignRecipient Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailCampaignRecipient : BaseEntity
{
    public Guid CampaignId { get; set; }
    public EmailCampaign Campaign { get; set; } = null!;
    public Guid SubscriberId { get; set; }
    public EmailSubscriber Subscriber { get; set; } = null!;
    public EmailRecipientStatus Status { get; set; } = EmailRecipientStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public DateTime? BouncedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int OpenCount { get; set; } = 0;
    public int ClickCount { get; set; } = 0;
}

