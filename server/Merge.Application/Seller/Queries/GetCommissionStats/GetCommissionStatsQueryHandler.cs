using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetCommissionStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCommissionStatsQueryHandler(IDbContext context, ILogger<GetCommissionStatsQueryHandler> logger) : IRequestHandler<GetCommissionStatsQuery, CommissionStatsDto>
{

    public async Task<CommissionStatsDto> Handle(GetCommissionStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting commission stats. SellerId: {SellerId}",
            request.SellerId?.ToString() ?? "All");

        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        IQueryable<SellerCommission> query = context.Set<SellerCommission>()
            .AsNoTracking();

        if (request.SellerId.HasValue)
        {
            query = query.Where(sc => sc.SellerId == request.SellerId.Value);
        }

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var now = DateTime.UtcNow.AddMonths(-12);
        
        var totalCommissions = await query.CountAsync(cancellationToken);
        var totalEarnings = await query.SumAsync(c => c.CommissionAmount, cancellationToken);
        var pendingCommissions = await query.Where(c => c.Status == CommissionStatus.Pending).SumAsync(c => c.NetAmount, cancellationToken);
        var approvedCommissions = await query.Where(c => c.Status == CommissionStatus.Approved).SumAsync(c => c.NetAmount, cancellationToken);
        var paidCommissions = await query.Where(c => c.Status == CommissionStatus.Paid).SumAsync(c => c.NetAmount, cancellationToken);
        var averageCommissionRate = totalCommissions > 0 ? await query.AverageAsync(c => c.CommissionRate, cancellationToken) : 0;
        var totalPlatformFees = await query.SumAsync(c => c.PlatformFee, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var commissionsByMonth = await query
            .Where(c => c.CreatedAt >= now)
            .GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new { Key = g.Key, Value = g.Sum(c => c.NetAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);

        return new CommissionStatsDto
        {
            TotalCommissions = totalCommissions,
            TotalEarnings = totalEarnings,
            PendingCommissions = pendingCommissions,
            ApprovedCommissions = approvedCommissions,
            PaidCommissions = paidCommissions,
            AvailableForPayout = approvedCommissions,
            AverageCommissionRate = averageCommissionRate,
            TotalPlatformFees = totalPlatformFees,
            CommissionsByMonth = commissionsByMonth
        };
    }
}
