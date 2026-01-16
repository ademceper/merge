namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Partial update DTO for Pre-Order Campaign (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchPreOrderCampaignDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? ExpectedDeliveryDate { get; init; }
    public int? MaxQuantity { get; init; }
    public decimal? DepositPercentage { get; init; }
    public decimal? SpecialPrice { get; init; }
}
