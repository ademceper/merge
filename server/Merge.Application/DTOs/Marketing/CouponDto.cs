using Merge.Domain.Modules.Marketing;
namespace Merge.Application.DTOs.Marketing;


public record CouponDto(
    Guid Id,
    string Code,
    string Description,
    decimal DiscountAmount,
    decimal? DiscountPercentage,
    decimal? MinimumPurchaseAmount,
    decimal? MaximumDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int UsageLimit,
    int UsedCount,
    bool IsActive,
    bool IsForNewUsersOnly,
    List<Guid>? ApplicableCategoryIds,
    List<Guid>? ApplicableProductIds);
