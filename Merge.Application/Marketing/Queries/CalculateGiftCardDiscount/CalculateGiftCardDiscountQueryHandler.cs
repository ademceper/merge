using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.CalculateGiftCardDiscount;

public class CalculateGiftCardDiscountQueryHandler : IRequestHandler<CalculateGiftCardDiscountQuery, decimal>
{
    private readonly IDbContext _context;
    private readonly ILogger<CalculateGiftCardDiscountQueryHandler> _logger;

    public CalculateGiftCardDiscountQueryHandler(
        IDbContext context,
        ILogger<CalculateGiftCardDiscountQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> Handle(CalculateGiftCardDiscountQuery request, CancellationToken cancellationToken)
    {
        var giftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        if (giftCard == null || !giftCard.IsActive || giftCard.IsRedeemed || DateTime.UtcNow > giftCard.ExpiresAt || giftCard.RemainingAmount <= 0)
        {
            return 0;
        }

        // Hediye kartı bakiyesi kadar indirim uygula
        var discount = Math.Min(giftCard.RemainingAmount, request.OrderAmount);
        
        _logger.LogInformation("GiftCard discount calculated. Code: {Code}, Discount: {Discount}", request.Code, discount);

        return discount;
    }
}
