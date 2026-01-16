namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Partial update DTO for Pick Pack Item Status (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchPickPackItemStatusDto
{
    public bool? IsPicked { get; init; }
    public bool? IsPacked { get; init; }
    public string? Location { get; init; }
}
