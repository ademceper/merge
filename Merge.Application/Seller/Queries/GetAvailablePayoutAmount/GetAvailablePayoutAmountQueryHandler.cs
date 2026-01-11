using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetAvailablePayoutAmount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAvailablePayoutAmountQueryHandler : IRequestHandler<GetAvailablePayoutAmountQuery, decimal>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetAvailablePayoutAmountQueryHandler> _logger;

    public GetAvailablePayoutAmountQueryHandler(
        IDbContext context,
        ILogger<GetAvailablePayoutAmountQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> Handle(GetAvailablePayoutAmountQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting available payout amount. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        var amount = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == request.SellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(sc => sc.NetAmount, cancellationToken);

        _logger.LogInformation("Available payout amount retrieved. SellerId: {SellerId}, Amount: {Amount}",
            request.SellerId, amount);

        return amount;
    }
}
