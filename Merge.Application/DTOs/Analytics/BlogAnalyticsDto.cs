namespace Merge.Application.DTOs.Analytics;

public class BlogAnalyticsDto
{
    public int TotalPosts { get; set; }
    public int PublishedPosts { get; set; }
    public int DraftPosts { get; set; }
    public int TotalViews { get; set; }
    public int TotalComments { get; set; }
    public Dictionary<string, int> PostsByCategory { get; set; } = new();
    public List<PopularPostDto> PopularPosts { get; set; } = new();
}
