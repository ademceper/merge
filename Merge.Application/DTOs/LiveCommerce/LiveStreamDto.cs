using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.LiveCommerce;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record LiveStreamDto(
    Guid Id,
    Guid SellerId,
    string SellerName,
    string Title,
    string Description,
    string Status,
    DateTime? ScheduledStartTime,
    DateTime? ActualStartTime,
    DateTime? EndTime,
    string? StreamUrl,
    string? StreamKey,
    string? ThumbnailUrl,
    int ViewerCount,
    int PeakViewerCount,
    int TotalViewerCount,
    int OrderCount,
    decimal Revenue,
    bool IsActive,
    string? Category,
    string? Tags,
    IReadOnlyList<LiveStreamProductDto> Products,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
