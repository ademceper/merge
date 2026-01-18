using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Marketing;


public record EmailCampaignStatsDto(
    int TotalCampaigns,
    int ActiveCampaigns,
    int TotalSubscribers,
    int ActiveSubscribers,
    long TotalEmailsSent,
    decimal AverageOpenRate,
    decimal AverageClickRate,
    List<EmailCampaignDto> RecentCampaigns);
