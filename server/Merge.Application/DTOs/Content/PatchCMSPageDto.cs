namespace Merge.Application.DTOs.Content;

/// <summary>
/// Partial update DTO for CMS Page (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCMSPageDto
{
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Excerpt { get; init; }
    public string? PageType { get; init; }
    public string? Status { get; init; }
    public string? Template { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public bool? IsHomePage { get; init; }
    public int? DisplayOrder { get; init; }
    public bool? ShowInMenu { get; init; }
    public string? MenuTitle { get; init; }
    public Guid? ParentPageId { get; init; }
}
