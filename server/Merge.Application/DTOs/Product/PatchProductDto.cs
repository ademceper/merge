namespace Merge.Application.DTOs.Product;

/// <summary>
/// Partial update DTO for Product (PATCH support)
/// All fields are optional for partial updates
/// </summary>
public record PatchProductDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? SKU { get; init; }
    public decimal? Price { get; init; }
    public decimal? DiscountPrice { get; init; }
    public int? StockQuantity { get; init; }
    public string? Brand { get; init; }
    public string? ImageUrl { get; init; }
    public List<string>? ImageUrls { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsActive { get; init; }
}
