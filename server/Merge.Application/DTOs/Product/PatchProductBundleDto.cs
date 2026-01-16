namespace Merge.Application.DTOs.Product;

/// <summary>
/// Partial update DTO for Product Bundle (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchProductBundleDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? BundlePrice { get; init; }
    public string? ImageUrl { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
