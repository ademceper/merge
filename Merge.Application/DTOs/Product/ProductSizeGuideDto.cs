namespace Merge.Application.DTOs.Product;

public class ProductSizeGuideDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public SizeGuideDto SizeGuide { get; set; } = null!;
    public string? CustomNotes { get; set; }
    public string? FitDescription { get; set; }
}
