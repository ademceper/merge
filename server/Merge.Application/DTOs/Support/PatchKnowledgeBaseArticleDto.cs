namespace Merge.Application.DTOs.Support;

/// <summary>
/// Partial update DTO for Knowledge Base Article (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchKnowledgeBaseArticleDto
{
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Excerpt { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Status { get; init; }
    public bool? IsFeatured { get; init; }
    public int? DisplayOrder { get; init; }
    public List<string>? Tags { get; init; }
}
