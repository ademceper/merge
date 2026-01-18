using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateCoupon;

public record CreateCouponCommand(
    string Code,
    string Description,
    decimal? DiscountAmount,
    decimal? DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate,
    int UsageLimit,
    decimal? MinimumPurchaseAmount,
    decimal? MaximumDiscountAmount,
    bool IsForNewUsersOnly,
    List<Guid>? ApplicableCategoryIds,
    List<Guid>? ApplicableProductIds) : IRequest<CouponDto>;
