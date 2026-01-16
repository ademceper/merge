namespace Merge.Application.DTOs.Content;

/// <summary>
/// Partial update DTO for Sitemap Entry (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchSitemapEntryDto
{
    public string? Url { get; init; }
    public decimal? Priority { get; init; }
    public string? ChangeFrequency { get; init; }
    public DateTime? LastModified { get; init; }
    public bool? IsActive { get; init; }
}
