namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Email Campaign Stats DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record EmailCampaignStatsDto(
    int TotalCampaigns,
    int ActiveCampaigns,
    int TotalSubscribers,
    int ActiveSubscribers,
    long TotalEmailsSent,
    decimal AverageOpenRate,
    decimal AverageClickRate,
    List<EmailCampaignDto> RecentCampaigns);
