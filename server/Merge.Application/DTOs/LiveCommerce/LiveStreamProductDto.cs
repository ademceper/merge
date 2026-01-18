namespace Merge.Application.DTOs.LiveCommerce;

public record LiveStreamProductDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal? ProductPrice,
    decimal? SpecialPrice,
    int DisplayOrder,
    bool IsHighlighted,
    DateTime? ShowcasedAt,
    int ViewCount,
    int ClickCount,
    int OrderCount,
    string? ShowcaseNotes);
