namespace Merge.Application.DTOs.Seller;

/// <summary>
/// Partial update DTO for Commission Settings (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCommissionSettingsDto
{
    public decimal? CustomCommissionRate { get; init; }
    public bool? UseCustomRate { get; init; }
    public decimal? MinimumPayoutAmount { get; init; }
    public string? PaymentMethod { get; init; }
    public string? PaymentDetails { get; init; }
}
