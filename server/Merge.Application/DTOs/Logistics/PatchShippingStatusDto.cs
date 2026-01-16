using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Partial update DTO for Shipping Status (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchShippingStatusDto
{
    [StringLength(50)]
    public string? Status { get; init; }
}
