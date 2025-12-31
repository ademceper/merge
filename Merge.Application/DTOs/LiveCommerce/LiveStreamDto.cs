namespace Merge.Application.DTOs.LiveCommerce;

public class LiveStreamDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? StreamUrl { get; set; }
    public string? StreamKey { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int ViewerCount { get; set; }
    public int PeakViewerCount { get; set; }
    public int TotalViewerCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public bool IsActive { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public List<LiveStreamProductDto> Products { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
