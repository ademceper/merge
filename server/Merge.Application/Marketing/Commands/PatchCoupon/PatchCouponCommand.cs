using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.PatchCoupon;

/// <summary>
/// PATCH command for partial coupon updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCouponCommand(
    Guid Id,
    PatchCouponDto PatchDto
) : IRequest<CouponDto>;
