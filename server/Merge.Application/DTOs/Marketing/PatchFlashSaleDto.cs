namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Partial update DTO for Flash Sale (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchFlashSaleDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool? IsActive { get; init; }
    public string? BannerImageUrl { get; init; }
}
