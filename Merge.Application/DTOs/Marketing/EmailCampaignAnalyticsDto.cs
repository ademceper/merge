namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Email Campaign Analytics DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record EmailCampaignAnalyticsDto(
    Guid CampaignId,
    string CampaignName,
    int TotalRecipients,
    int SentCount,
    int DeliveredCount,
    int OpenedCount,
    int ClickedCount,
    int BouncedCount,
    int UnsubscribedCount,
    decimal OpenRate,
    decimal ClickRate,
    decimal BounceRate,
    decimal UnsubscribeRate,
    DateTime? SentAt);
