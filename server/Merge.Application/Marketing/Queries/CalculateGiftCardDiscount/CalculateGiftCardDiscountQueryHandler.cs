using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.CalculateGiftCardDiscount;

public class CalculateGiftCardDiscountQueryHandler(
    IDbContext context,
    ILogger<CalculateGiftCardDiscountQueryHandler> logger) : IRequestHandler<CalculateGiftCardDiscountQuery, decimal>
{
    public async Task<decimal> Handle(CalculateGiftCardDiscountQuery request, CancellationToken cancellationToken)
    {
        var giftCard = await context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        if (giftCard == null || !giftCard.IsActive || giftCard.IsRedeemed || DateTime.UtcNow > giftCard.ExpiresAt || giftCard.RemainingAmount <= 0)
        {
            return 0;
        }

        // Hediye kartı bakiyesi kadar indirim uygula
        var discount = Math.Min(giftCard.RemainingAmount, request.OrderAmount);
        
        logger.LogInformation("GiftCard discount calculated. Code: {Code}, Discount: {Discount}", request.Code, discount);

        return discount;
    }
}
