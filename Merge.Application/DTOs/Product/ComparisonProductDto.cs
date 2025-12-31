namespace Merge.Application.DTOs.Product;

public class ComparisonProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? MainImage { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal? Rating { get; set; }
    public int ReviewCount { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public int Position { get; set; }
}
