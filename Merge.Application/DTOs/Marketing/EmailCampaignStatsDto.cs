namespace Merge.Application.DTOs.Marketing;

public class EmailCampaignStatsDto
{
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int TotalSubscribers { get; set; }
    public int ActiveSubscribers { get; set; }
    public long TotalEmailsSent { get; set; }
    public decimal AverageOpenRate { get; set; }
    public decimal AverageClickRate { get; set; }
    public List<EmailCampaignDto> RecentCampaigns { get; set; } = new();
}
