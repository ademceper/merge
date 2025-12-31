namespace Merge.Application.DTOs.Marketing;

public class FlashSaleProductDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockLimit { get; set; }
    public int SoldQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int SortOrder { get; set; }
}
