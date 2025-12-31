namespace Merge.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty; // Renk, Beden, Model vb.
    public string Value { get; set; } = string.Empty; // Kırmızı, XL, 2024 vb.
    public string? SKU { get; set; }
    public decimal? Price { get; set; } // Varyant özel fiyat
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    
    // Navigation properties
    public Product Product { get; set; } = null!;
}

