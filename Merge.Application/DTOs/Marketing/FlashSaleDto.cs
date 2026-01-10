namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Flash Sale DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record FlashSaleDto(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    string? BannerImageUrl,
    List<FlashSaleProductDto> Products)
{
    public bool IsOngoing => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate && IsActive;
}
