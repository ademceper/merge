namespace Merge.Application.DTOs.Content;

/// <summary>
/// Partial update DTO for Page Builder (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchPageBuilderDto
{
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Template { get; init; }
    public string? PageType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? OgImageUrl { get; init; }
}
