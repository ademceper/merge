namespace Merge.Application.DTOs.User;

public class ActivityStatsDto
{
    public int TotalActivities { get; set; }
    public int UniqueUsers { get; set; }
    public Dictionary<string, int> ActivitiesByType { get; set; } = new();
    public Dictionary<string, int> ActivitiesByDevice { get; set; } = new();
    public Dictionary<string, int> ActivitiesByHour { get; set; } = new();
    public List<TopUserActivityDto> TopUsers { get; set; } = new();
    public List<PopularProductDto> MostViewedProducts { get; set; } = new();
    public decimal AverageSessionDuration { get; set; }
}
