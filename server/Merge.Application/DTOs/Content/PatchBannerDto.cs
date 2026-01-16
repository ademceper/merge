namespace Merge.Application.DTOs.Content;

/// <summary>
/// Partial update DTO for Banner (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchBannerDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? LinkUrl { get; init; }
    public string? Position { get; init; }
    public int? SortOrder { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? ProductId { get; init; }
}
