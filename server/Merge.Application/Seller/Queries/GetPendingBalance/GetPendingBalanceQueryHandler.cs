using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetPendingBalance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPendingBalanceQueryHandler(IDbContext context, ILogger<GetPendingBalanceQueryHandler> logger) : IRequestHandler<GetPendingBalanceQuery, decimal>
{

    public async Task<decimal> Handle(GetPendingBalanceQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting pending balance. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        return seller?.PendingBalance ?? 0;
    }
}
