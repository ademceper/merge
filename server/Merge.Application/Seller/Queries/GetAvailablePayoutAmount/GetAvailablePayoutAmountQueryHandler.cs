using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetAvailablePayoutAmount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAvailablePayoutAmountQueryHandler(IDbContext context, ILogger<GetAvailablePayoutAmountQueryHandler> logger) : IRequestHandler<GetAvailablePayoutAmountQuery, decimal>
{

    public async Task<decimal> Handle(GetAvailablePayoutAmountQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting available payout amount. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        var amount = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == request.SellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(sc => sc.NetAmount, cancellationToken);

        logger.LogInformation("Available payout amount retrieved. SellerId: {SellerId}, Amount: {Amount}",
            request.SellerId, amount);

        return amount;
    }
}
