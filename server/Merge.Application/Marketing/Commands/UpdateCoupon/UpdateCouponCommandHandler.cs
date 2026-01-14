using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UpdateCoupon;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class UpdateCouponCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateCouponCommandHandler> logger) : IRequestHandler<UpdateCouponCommand, CouponDto>
{
    public async Task<CouponDto> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating coupon. CouponId: {CouponId}", request.Id);

        var coupon = await context.Set<Coupon>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (coupon == null)
        {
            logger.LogWarning("Coupon not found. CouponId: {CouponId}", request.Id);
            throw new NotFoundException("Kupon", request.Id);
        }

        // ✅ PERFORMANCE: AsNoTracking - Check if code already exists (if changed)
        if (coupon.Code.ToUpper() != request.Code.ToUpper())
        {
            var existingCoupon = await context.Set<Coupon>()
                .AsNoTracking()
                .AnyAsync(c => c.Code.ToUpper() == request.Code.ToUpper() && c.Id != request.Id, cancellationToken);

            if (existingCoupon)
            {
                logger.LogWarning("Coupon with code already exists. Code: {Code}", request.Code);
                throw new BusinessException($"Bu kupon kodu zaten kullanılıyor: '{request.Code}'");
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        coupon.UpdateCode(request.Code);
        coupon.UpdateDescription(request.Description);

        if (request.DiscountAmount.HasValue && request.DiscountAmount.Value > 0)
        {
            coupon.SetDiscountAmount(new Money(request.DiscountAmount.Value));
        }
        else
        {
            coupon.SetDiscountAmount(null);
        }

        if (request.DiscountPercentage.HasValue)
        {
            coupon.SetDiscountPercentage(new Percentage(request.DiscountPercentage.Value));
        }
        else
        {
            coupon.SetDiscountPercentage(null);
        }

        if (request.MinimumPurchaseAmount.HasValue)
        {
            coupon.SetMinimumPurchaseAmount(new Money(request.MinimumPurchaseAmount.Value));
        }
        else
        {
            coupon.SetMinimumPurchaseAmount(null);
        }

        if (request.MaximumDiscountAmount.HasValue)
        {
            coupon.SetMaximumDiscountAmount(new Money(request.MaximumDiscountAmount.Value));
        }
        else
        {
            coupon.SetMaximumDiscountAmount(null);
        }

        coupon.UpdateDates(request.StartDate, request.EndDate);
        coupon.SetUsageLimit(request.UsageLimit);
        coupon.SetApplicableCategoryIds(request.ApplicableCategoryIds);
        coupon.SetApplicableProductIds(request.ApplicableProductIds);
        coupon.SetForNewUsersOnly(request.IsForNewUsersOnly);

        if (request.IsActive)
        {
            coupon.Activate();
        }
        else
        {
            coupon.Deactivate();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking ile tek query'de getir
        var updatedCoupon = await context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == coupon.Id, cancellationToken);

        if (updatedCoupon == null)
        {
            logger.LogWarning("Coupon not found after update. CouponId: {CouponId}", coupon.Id);
            throw new NotFoundException("Kupon", coupon.Id);
        }

        logger.LogInformation("Coupon updated successfully. CouponId: {CouponId}", request.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<CouponDto>(updatedCoupon);
    }
}
