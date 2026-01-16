namespace Merge.Application.DTOs.Content;

/// <summary>
/// Partial update DTO for Blog Category (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchBlogCategoryDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public string? ImageUrl { get; init; }
    public int? DisplayOrder { get; init; }
    public bool? IsActive { get; init; }
}
