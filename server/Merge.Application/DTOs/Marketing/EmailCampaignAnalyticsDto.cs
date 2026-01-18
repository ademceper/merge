using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Marketing;


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
