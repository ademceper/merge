namespace Merge.Application.DTOs.LiveCommerce;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
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
