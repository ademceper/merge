namespace Merge.Application.DTOs.Product;

/// <summary>
/// Partial update DTO for Product Template (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchProductTemplateDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Brand { get; init; }
    public string? DefaultSKUPrefix { get; init; }
    public decimal? DefaultPrice { get; init; }
    public int? DefaultStockQuantity { get; init; }
    public string? DefaultImageUrl { get; init; }
    public Dictionary<string, string>? Specifications { get; init; }
    public Dictionary<string, string>? Attributes { get; init; }
    public bool? IsActive { get; init; }
}
