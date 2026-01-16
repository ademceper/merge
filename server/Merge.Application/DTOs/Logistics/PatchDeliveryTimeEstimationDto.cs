namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Partial update DTO for Delivery Time Estimation (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchDeliveryTimeEstimationDto
{
    public int? MinDays { get; init; }
    public int? MaxDays { get; init; }
    public int? AverageDays { get; init; }
    public bool? IsActive { get; init; }
    public DeliveryTimeSettingsDto? Conditions { get; init; }
}
