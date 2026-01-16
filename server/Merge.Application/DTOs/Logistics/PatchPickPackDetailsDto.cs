namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Partial update DTO for Pick Pack Details (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchPickPackDetailsDto
{
    public string? Notes { get; init; }
    public decimal? Weight { get; init; }
    public string? Dimensions { get; init; }
    public int? PackageCount { get; init; }
}
