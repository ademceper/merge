namespace Merge.Application.DTOs.Product;

public class ProductBundleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BundlePrice { get; set; }
    public decimal? OriginalTotalPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<BundleItemDto> Items { get; set; } = new List<BundleItemDto>();
    public bool IsAvailable => IsActive && 
        (!StartDate.HasValue || DateTime.UtcNow >= StartDate.Value) &&
        (!EndDate.HasValue || DateTime.UtcNow <= EndDate.Value);
}
