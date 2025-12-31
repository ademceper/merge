namespace Merge.Application.DTOs.Marketing;

public class FlashSaleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? BannerImageUrl { get; set; }
    public List<FlashSaleProductDto> Products { get; set; } = new List<FlashSaleProductDto>();
    public bool IsOngoing => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate && IsActive;
}
