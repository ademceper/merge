using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.PatchCoupon;

/// <summary>
/// Handler for PatchCouponCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchCouponCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchCouponCommandHandler> logger) : IRequestHandler<PatchCouponCommand, CouponDto>
{
    public async Task<CouponDto> Handle(PatchCouponCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching coupon. CouponId: {CouponId}", request.Id);

        var coupon = await context.Set<Coupon>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (coupon is null)
        {
            logger.LogWarning("Coupon not found. CouponId: {CouponId}", request.Id);
            throw new NotFoundException("Kupon", request.Id);
        }

        // Apply partial updates - only update fields that are provided
        if (request.PatchDto.Code is not null)
        {
            if (!string.Equals(coupon.Code, request.PatchDto.Code, StringComparison.OrdinalIgnoreCase))
            {
                var existingCoupon = await context.Set<Coupon>()
                    .AsNoTracking()
                    .AnyAsync(c => EF.Functions.ILike(c.Code, request.PatchDto.Code) && c.Id != request.Id, cancellationToken);

                if (existingCoupon)
                {
                    logger.LogWarning("Coupon with code already exists. Code: {Code}", request.PatchDto.Code);
                    throw new BusinessException($"Bu kupon kodu zaten kullanılıyor: '{request.PatchDto.Code}'");
                }
            }
            coupon.UpdateCode(request.PatchDto.Code);
        }

        if (request.PatchDto.Description is not null)
        {
            coupon.UpdateDescription(request.PatchDto.Description);
        }

        if (request.PatchDto.DiscountAmount.HasValue)
        {
            if (request.PatchDto.DiscountAmount.Value > 0)
            {
                coupon.SetDiscountAmount(new Money(request.PatchDto.DiscountAmount.Value));
            }
            else
            {
                coupon.SetDiscountAmount(null);
            }
        }

        if (request.PatchDto.DiscountPercentage.HasValue)
        {
            coupon.SetDiscountPercentage(new Percentage(request.PatchDto.DiscountPercentage.Value));
        }
        else if (request.PatchDto.DiscountPercentage is null && request.PatchDto.DiscountAmount is null)
        {
            // Only clear if explicitly set to null and discount amount is not being updated
        }

        if (request.PatchDto.MinimumPurchaseAmount.HasValue)
        {
            coupon.SetMinimumPurchaseAmount(new Money(request.PatchDto.MinimumPurchaseAmount.Value));
        }

        if (request.PatchDto.MaximumDiscountAmount.HasValue)
        {
            coupon.SetMaximumDiscountAmount(new Money(request.PatchDto.MaximumDiscountAmount.Value));
        }

        if (request.PatchDto.StartDate.HasValue || request.PatchDto.EndDate.HasValue)
        {
            var startDate = request.PatchDto.StartDate ?? coupon.StartDate;
            var endDate = request.PatchDto.EndDate ?? coupon.EndDate;
            coupon.UpdateDates(startDate, endDate);
        }

        if (request.PatchDto.UsageLimit.HasValue)
        {
            coupon.SetUsageLimit(request.PatchDto.UsageLimit.Value);
        }

        if (request.PatchDto.ApplicableCategoryIds is not null)
        {
            coupon.SetApplicableCategoryIds(request.PatchDto.ApplicableCategoryIds);
        }

        if (request.PatchDto.ApplicableProductIds is not null)
        {
            coupon.SetApplicableProductIds(request.PatchDto.ApplicableProductIds);
        }

        if (request.PatchDto.IsForNewUsersOnly.HasValue)
        {
            coupon.SetForNewUsersOnly(request.PatchDto.IsForNewUsersOnly.Value);
        }

        if (request.PatchDto.IsActive.HasValue)
        {
            if (request.PatchDto.IsActive.Value)
            {
                coupon.Activate();
            }
            else
            {
                coupon.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedCoupon = await context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == coupon.Id, cancellationToken);

        if (updatedCoupon is null)
        {
            logger.LogWarning("Coupon not found after patch. CouponId: {CouponId}", coupon.Id);
            throw new NotFoundException("Kupon", coupon.Id);
        }

        logger.LogInformation("Coupon patched successfully. CouponId: {CouponId}", request.Id);

        return mapper.Map<CouponDto>(updatedCoupon);
    }
}
