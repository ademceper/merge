namespace Merge.Application.DTOs.LiveCommerce;

/// <summary>
/// Partial update DTO for Live Stream (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchLiveStreamDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? ScheduledStartTime { get; init; }
    public string? StreamUrl { get; init; }
    public string? StreamKey { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? Category { get; init; }
    public string? Tags { get; init; }
}
