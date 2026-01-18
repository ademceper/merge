namespace Merge.Application.DTOs.Marketing;


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
