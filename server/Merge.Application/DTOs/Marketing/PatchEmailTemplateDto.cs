namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Partial update DTO for Email Template (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchEmailTemplateDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Subject { get; init; }
    public string? HtmlContent { get; init; }
    public string? TextContent { get; init; }
    public string? Type { get; init; }
    public List<string>? Variables { get; init; }
    public string? Thumbnail { get; init; }
    public bool? IsActive { get; init; }
}
