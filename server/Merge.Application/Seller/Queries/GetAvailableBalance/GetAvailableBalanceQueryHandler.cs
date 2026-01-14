using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetAvailableBalance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAvailableBalanceQueryHandler : IRequestHandler<GetAvailableBalanceQuery, decimal>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetAvailableBalanceQueryHandler> _logger;

    public GetAvailableBalanceQueryHandler(
        IDbContext context,
        ILogger<GetAvailableBalanceQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> Handle(GetAvailableBalanceQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting available balance. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        return seller?.AvailableBalance ?? 0;
    }
}
