namespace Merge.Application.DTOs.Payment;

/// <summary>
/// Partial update DTO for Payment Method (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchPaymentMethodDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? IconUrl { get; init; }
    public bool? IsActive { get; init; }
    public bool? RequiresOnlinePayment { get; init; }
    public bool? RequiresManualVerification { get; init; }
    public decimal? MinimumAmount { get; init; }
    public decimal? MaximumAmount { get; init; }
    public decimal? ProcessingFee { get; init; }
    public decimal? ProcessingFeePercentage { get; init; }
    public PaymentMethodSettingsDto? Settings { get; init; }
    public int? DisplayOrder { get; init; }
    public bool? IsDefault { get; init; }
}
