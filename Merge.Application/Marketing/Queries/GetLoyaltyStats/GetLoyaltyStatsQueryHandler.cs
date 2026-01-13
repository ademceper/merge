using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetLoyaltyStats;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetLoyaltyStatsQueryHandler(IDbContext context) : IRequestHandler<GetLoyaltyStatsQuery, LoyaltyStatsDto>
{
    public async Task<LoyaltyStatsDto> Handle(GetLoyaltyStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var totalMembers = await context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalPointsIssued = await context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .SumAsync(a => (long)a.LifetimePoints, cancellationToken);

        var totalPointsRedeemed = await context.Set<LoyaltyTransaction>()
            .AsNoTracking()
            .Where(t => t.Points < 0)
            .SumAsync(t => (long)Math.Abs(t.Points), cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // NOT: GroupBy ile Include birlikte kullanıldığında AsSplitQuery kullanılamaz
        // Bu durumda Join kullanarak Tier bilgisini alıyoruz
        var membersByTierId = await context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .GroupBy(a => a.TierId)
            .Select(g => new { TierId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        
        // Tier isimlerini almak için ayrı bir sorgu
        var tierIds = membersByTierId.Where(x => x.TierId.HasValue).Select(x => x.TierId!.Value).ToList();
        var tierNames = await context.Set<LoyaltyTier>()
            .AsNoTracking()
            .Where(t => tierIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);
        
        // Dictionary'yi Tier isimleriyle oluştur
        var membersByTier = membersByTierId.ToDictionary(
            kvp => kvp.TierId.HasValue && tierNames.TryGetValue(kvp.TierId.Value, out var name) 
                ? name 
                : "No Tier",
            kvp => kvp.Count);

        return new LoyaltyStatsDto(
            totalMembers,
            totalPointsIssued,
            totalPointsRedeemed,
            membersByTier,
            totalMembers > 0 ? (decimal)totalPointsIssued / totalMembers : 0);
    }
}
