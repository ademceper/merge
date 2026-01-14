using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetPickPackStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetPickPackStatsQueryHandler(
    IDbContext context,
    ILogger<GetPickPackStatsQueryHandler> logger) : IRequestHandler<GetPickPackStatsQuery, Dictionary<string, int>>
{

    public async Task<Dictionary<string, int>> Handle(GetPickPackStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting pick-pack stats. WarehouseId: {WarehouseId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.WarehouseId, request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var query = context.Set<PickPack>()
            .AsNoTracking();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(pp => pp.WarehouseId == request.WarehouseId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(pp => pp.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(pp => pp.CreatedAt <= request.EndDate.Value);
        }

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Tek sorguda tüm status'leri GroupBy ile al (8 ayrı CountAsync yerine)
        var total = await query.CountAsync(cancellationToken);
        
        var statusCounts = await query
            .GroupBy(pp => pp.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece Dictionary oluşturma)
        // ✅ PERFORMANCE: ToDictionary kullanarak FirstOrDefault overhead'ini önle
        var statusCountsDict = statusCounts.ToDictionary(s => s.Status, s => s.Count);
        
        // ✅ BOLUM 7.1.9: Collection Expressions (C# 12) - Modern C# feature kullanımı
        Dictionary<string, int> result = new()
        {
            ["Total"] = total,
            ["Pending"] = statusCountsDict.GetValueOrDefault(PickPackStatus.Pending, 0),
            ["Picking"] = statusCountsDict.GetValueOrDefault(PickPackStatus.Picking, 0),
            ["Picked"] = statusCountsDict.GetValueOrDefault(PickPackStatus.Picked, 0),
            ["Packing"] = statusCountsDict.GetValueOrDefault(PickPackStatus.Packing, 0),
            ["Packed"] = statusCountsDict.GetValueOrDefault(PickPackStatus.Packed, 0),
            ["Shipped"] = statusCountsDict.GetValueOrDefault(PickPackStatus.Shipped, 0),
            ["Cancelled"] = statusCountsDict.GetValueOrDefault(PickPackStatus.Cancelled, 0)
        };

        return result;
    }
}

