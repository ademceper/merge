namespace Merge.Application.DTOs.LiveCommerce;

public record LiveStreamStatsDto(
    Guid StreamId,
    int ViewerCount,
    int PeakViewerCount,
    int TotalViewerCount,
    int OrderCount,
    decimal Revenue,
    decimal TotalRevenue,
    string Status,
    int Duration);
