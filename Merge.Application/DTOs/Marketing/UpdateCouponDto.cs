using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Update Coupon DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record UpdateCouponDto
{
    [Required(ErrorMessage = "Coupon code is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Coupon code must be between 1 and 50 characters")]
    public string Code { get; init; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, 999999999.99, ErrorMessage = "Discount amount must be a positive value")]
    public decimal? DiscountAmount { get; init; }

    [Range(0.01, 100, ErrorMessage = "Discount percentage must be between 0.01 and 100")]
    public decimal? DiscountPercentage { get; init; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; init; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Usage limit must be a non-negative value")]
    public int UsageLimit { get; init; }

    [Range(0.01, 999999999.99, ErrorMessage = "Minimum purchase amount must be a positive value")]
    public decimal? MinimumPurchaseAmount { get; init; }

    [Range(0.01, 999999999.99, ErrorMessage = "Maximum discount amount must be a positive value")]
    public decimal? MaximumDiscountAmount { get; init; }

    public bool IsActive { get; init; }

    public bool IsForNewUsersOnly { get; init; }

    public List<Guid>? ApplicableCategoryIds { get; init; }

    public List<Guid>? ApplicableProductIds { get; init; }
}
