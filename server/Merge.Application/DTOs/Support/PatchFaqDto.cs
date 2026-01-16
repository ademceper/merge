namespace Merge.Application.DTOs.Support;

/// <summary>
/// Partial update DTO for FAQ (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchFaqDto
{
    public string? Question { get; init; }
    public string? Answer { get; init; }
    public string? Category { get; init; }
    public int? SortOrder { get; init; }
    public bool? IsPublished { get; init; }
}
