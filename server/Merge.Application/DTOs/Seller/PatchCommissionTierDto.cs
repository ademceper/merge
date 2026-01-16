namespace Merge.Application.DTOs.Seller;

/// <summary>
/// Partial update DTO for Commission Tier (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCommissionTierDto
{
    public string? Name { get; init; }
    public decimal? MinSales { get; init; }
    public decimal? MaxSales { get; init; }
    public decimal? CommissionRate { get; init; }
    public decimal? PlatformFeeRate { get; init; }
    public int? Priority { get; init; }
}
