namespace Merge.Application.DTOs.Content;

/// <summary>
/// Partial update DTO for Blog Post (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchBlogPostDto
{
    public Guid? CategoryId { get; init; }
    public string? Title { get; init; }
    public string? Excerpt { get; init; }
    public string? Content { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string? Status { get; init; }
    public List<string>? Tags { get; init; }
    public bool? IsFeatured { get; init; }
    public bool? AllowComments { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public string? OgImageUrl { get; init; }
}
