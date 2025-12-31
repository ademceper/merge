namespace Merge.Application.DTOs.Product;

public class ProductTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? DefaultSKUPrefix { get; set; }
    public decimal? DefaultPrice { get; set; }
    public int? DefaultStockQuantity { get; set; }
    public string? DefaultImageUrl { get; set; }
    public Dictionary<string, string>? Specifications { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
