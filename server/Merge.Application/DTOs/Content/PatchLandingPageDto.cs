namespace Merge.Application.DTOs.Content;

/// <summary>
/// Partial update DTO for Landing Page (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchLandingPageDto
{
    public string? Name { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Template { get; init; }
    public string? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? OgImageUrl { get; init; }
    public bool? EnableABTesting { get; init; }
    public int? TrafficSplit { get; init; }
}
