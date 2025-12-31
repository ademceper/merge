namespace Merge.Application.DTOs.LiveCommerce;

public class LiveStreamStatsDto
{
    public Guid StreamId { get; set; }
    public int ViewerCount { get; set; }
    public int PeakViewerCount { get; set; }
    public int TotalViewerCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Duration { get; set; } // In seconds
}
