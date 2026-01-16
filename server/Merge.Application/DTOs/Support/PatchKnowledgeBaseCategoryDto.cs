namespace Merge.Application.DTOs.Support;

/// <summary>
/// Partial update DTO for Knowledge Base Category (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchKnowledgeBaseCategoryDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public int? DisplayOrder { get; init; }
    public bool? IsActive { get; init; }
    public string? IconUrl { get; init; }
}
