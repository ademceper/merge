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

namespace Merge.Application.Marketing.Commands.CreateCoupon;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CreateCouponCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateCouponCommandHandler> logger) : IRequestHandler<CreateCouponCommand, CouponDto>
{

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating coupon. Code: {Code}", request.Code);

        // ✅ PERFORMANCE: AsNoTracking - Check if code already exists
        var existingCoupon = await context.Set<Coupon>()
            .AsNoTracking()
            .AnyAsync(c => c.Code.ToUpper() == request.Code.ToUpper(), cancellationToken);

        if (existingCoupon)
        {
            logger.LogWarning("Coupon with code already exists. Code: {Code}", request.Code);
            throw new BusinessException($"Bu kupon kodu zaten kullanılıyor: '{request.Code}'");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        Money? discountAmount = request.DiscountAmount.HasValue && request.DiscountAmount.Value > 0
            ? new Money(request.DiscountAmount.Value)
            : null;

        Percentage? discountPercentage = request.DiscountPercentage.HasValue
            ? new Percentage(request.DiscountPercentage.Value)
            : null;

        Money? minimumPurchaseAmount = request.MinimumPurchaseAmount.HasValue
            ? new Money(request.MinimumPurchaseAmount.Value)
            : null;

        Money? maximumDiscountAmount = request.MaximumDiscountAmount.HasValue
            ? new Money(request.MaximumDiscountAmount.Value)
            : null;

        var coupon = Coupon.Create(
            request.Code,
            request.Description,
            discountAmount,
            discountPercentage,
            request.StartDate,
            request.EndDate,
            request.UsageLimit,
            minimumPurchaseAmount,
            maximumDiscountAmount,
            request.IsForNewUsersOnly);

        if (request.ApplicableCategoryIds != null && request.ApplicableCategoryIds.Any())
        {
            coupon.SetApplicableCategoryIds(request.ApplicableCategoryIds);
        }

        if (request.ApplicableProductIds != null && request.ApplicableProductIds.Any())
        {
            coupon.SetApplicableProductIds(request.ApplicableProductIds);
        }

        await context.Set<Coupon>().AddAsync(coupon, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking ile tek query'de getir
        var createdCoupon = await context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == coupon.Id, cancellationToken);

        if (createdCoupon == null)
        {
            logger.LogWarning("Coupon not found after creation. CouponId: {CouponId}", coupon.Id);
            throw new NotFoundException("Kupon", coupon.Id);
        }

        logger.LogInformation("Coupon created successfully. CouponId: {CouponId}, Code: {Code}", coupon.Id, request.Code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<CouponDto>(createdCoupon);
    }
}
