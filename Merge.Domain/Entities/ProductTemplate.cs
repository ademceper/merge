namespace Merge.Domain.Entities;

public class ProductTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? DefaultSKUPrefix { get; set; }
    public decimal? DefaultPrice { get; set; }
    public int? DefaultStockQuantity { get; set; }
    public string? DefaultImageUrl { get; set; }
    public string? Specifications { get; set; } // JSON for default specifications
    public string? Attributes { get; set; } // JSON for default attributes
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0; // How many times this template was used
    
    // Navigation properties
    public Category Category { get; set; } = null!;
}

