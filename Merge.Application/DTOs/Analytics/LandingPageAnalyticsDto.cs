namespace Merge.Application.DTOs.Analytics;

public class LandingPageAnalyticsDto
{
    public Guid LandingPageId { get; set; }
    public string LandingPageName { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalConversions { get; set; }
    public decimal ConversionRate { get; set; }
    public Dictionary<string, int> ViewsByDate { get; set; } = new();
    public Dictionary<string, int> ConversionsByDate { get; set; } = new();
    public List<LandingPageVariantDto> Variants { get; set; } = new();
}
