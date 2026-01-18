using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.ValidateCoupon;

public class ValidateCouponCommandHandler(
    IDbContext context,
    ILogger<ValidateCouponCommandHandler> logger) : IRequestHandler<ValidateCouponCommand, decimal>
{

    public async Task<decimal> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderAmount <= 0)
        {
            throw new ValidationException("Sipariş tutarı 0'dan büyük olmalıdır.");
        }

        var coupon = await context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Code, request.Code), cancellationToken);

        if (coupon is null)
        {
            logger.LogWarning("Coupon not found. Code: {Code}", request.Code);
            throw new NotFoundException("Kupon", Guid.Empty);
        }

        if (!coupon.IsValid())
        {
            throw new BusinessException("Kupon geçerli değil.");
        }

        if (!coupon.CanBeUsedFor(request.OrderAmount))
        {
            throw new BusinessException($"Minimum alışveriş tutarı {coupon.MinimumPurchaseAmount:C} olmalıdır.");
        }

        // Yeni kullanıcı kontrolü
        if (coupon.IsForNewUsersOnly && request.UserId.HasValue)
        {
            var hasOrder = await context.Set<OrderEntity>()
                .AsNoTracking()
                .AnyAsync(o => o.UserId == request.UserId.Value, cancellationToken);

            if (hasOrder)
            {
                throw new BusinessException("Bu kupon sadece yeni kullanıcılar için geçerlidir.");
            }
        }

        // Kategori/Ürün kontrolü
        if (request.ProductIds is not null && request.ProductIds.Any())
        {
            if (coupon.ApplicableProductIds is not null && coupon.ApplicableProductIds.Any())
            {
                var hasApplicableProduct = request.ProductIds.Any(id => coupon.ApplicableProductIds.Contains(id));
                if (!hasApplicableProduct)
                {
                    throw new BusinessException("Bu kupon seçilen ürünler için geçerli değil.");
                }
            }
        }

        var purchaseAmount = new Money(request.OrderAmount);
        var discount = coupon.CalculateDiscount(purchaseAmount);

        logger.LogInformation("Coupon validated successfully. Code: {Code}, Discount: {Discount}", request.Code, discount.Amount);

        return discount.Amount;
    }
}
