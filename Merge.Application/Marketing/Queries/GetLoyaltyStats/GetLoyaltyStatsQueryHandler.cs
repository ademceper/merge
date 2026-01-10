using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetLoyaltyStats;

public class GetLoyaltyStatsQueryHandler : IRequestHandler<GetLoyaltyStatsQuery, LoyaltyStatsDto>
{
    private readonly IDbContext _context;

    public GetLoyaltyStatsQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<LoyaltyStatsDto> Handle(GetLoyaltyStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var totalMembers = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalPointsIssued = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .SumAsync(a => (long)a.LifetimePoints, cancellationToken);

        var totalPointsRedeemed = await _context.Set<LoyaltyTransaction>()
            .AsNoTracking()
            .Where(t => t.Points < 0)
            .SumAsync(t => (long)Math.Abs(t.Points), cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var membersByTier = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .Include(a => a.Tier)
            .GroupBy(a => a.Tier != null ? a.Tier.Name : "No Tier")
            .Select(g => new { Tier = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Tier, x => x.Count, cancellationToken);

        return new LoyaltyStatsDto(
            totalMembers,
            totalPointsIssued,
            totalPointsRedeemed,
            membersByTier,
            totalMembers > 0 ? (decimal)totalPointsIssued / totalMembers : 0);
    }
}
