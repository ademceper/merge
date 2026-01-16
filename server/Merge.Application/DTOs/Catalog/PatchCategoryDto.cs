namespace Merge.Application.DTOs.Catalog;

/// <summary>
/// Partial update DTO for Category (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCategoryDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Slug { get; init; }
    public string? ImageUrl { get; init; }
    public Guid? ParentCategoryId { get; init; }
}
