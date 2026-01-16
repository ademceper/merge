namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Partial update DTO for Coupon (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCouponDto
{
    public string? Code { get; init; }
    public string? Description { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UsageLimit { get; init; }
    public decimal? MinimumPurchaseAmount { get; init; }
    public decimal? MaximumDiscountAmount { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsForNewUsersOnly { get; init; }
    public List<Guid>? ApplicableCategoryIds { get; init; }
    public List<Guid>? ApplicableProductIds { get; init; }
}
