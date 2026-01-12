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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ValidateCouponCommandHandler : IRequestHandler<ValidateCouponCommand, decimal>
{
    private readonly IDbContext _context;
    private readonly ILogger<ValidateCouponCommandHandler> _logger;

    public ValidateCouponCommandHandler(
        IDbContext context,
        ILogger<ValidateCouponCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderAmount <= 0)
        {
            throw new ValidationException("Sipariş tutarı 0'dan büyük olmalıdır.");
        }

        var coupon = await _context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.Code.ToUpper(), cancellationToken);

        if (coupon == null)
        {
            _logger.LogWarning("Coupon not found. Code: {Code}", request.Code);
            throw new NotFoundException("Kupon", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
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
            var hasOrder = await _context.Set<OrderEntity>()
                .AsNoTracking()
                .AnyAsync(o => o.UserId == request.UserId.Value, cancellationToken);

            if (hasOrder)
            {
                throw new BusinessException("Bu kupon sadece yeni kullanıcılar için geçerlidir.");
            }
        }

        // Kategori/Ürün kontrolü
        if (request.ProductIds != null && request.ProductIds.Any())
        {
            if (coupon.ApplicableProductIds != null && coupon.ApplicableProductIds.Any())
            {
                var hasApplicableProduct = request.ProductIds.Any(id => coupon.ApplicableProductIds.Contains(id));
                if (!hasApplicableProduct)
                {
                    throw new BusinessException("Bu kupon seçilen ürünler için geçerli değil.");
                }
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        var purchaseAmount = new Money(request.OrderAmount);
        var discount = coupon.CalculateDiscount(purchaseAmount);

        _logger.LogInformation("Coupon validated successfully. Code: {Code}, Discount: {Discount}", request.Code, discount.Amount);

        return discount.Amount;
    }
}
