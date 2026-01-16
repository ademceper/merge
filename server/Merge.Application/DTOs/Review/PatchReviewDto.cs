namespace Merge.Application.DTOs.Review;

/// <summary>
/// Partial update DTO for Review (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchReviewDto
{
    public int? Rating { get; init; }
    public string? Title { get; init; }
    public string? Comment { get; init; }
}
