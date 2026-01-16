namespace Merge.Application.DTOs.Support;

/// <summary>
/// Partial update DTO for Customer Communication Status (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCommunicationStatusDto
{
    public string? Status { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReadAt { get; init; }
}
