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
public class GetPickPackStatsQueryHandler : IRequestHandler<GetPickPackStatsQuery, Dictionary<string, int>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetPickPackStatsQueryHandler> _logger;

    public GetPickPackStatsQueryHandler(
        IDbContext context,
        ILogger<GetPickPackStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Dictionary<string, int>> Handle(GetPickPackStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting pick-pack stats. WarehouseId: {WarehouseId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.WarehouseId, request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var query = _context.Set<PickPack>()
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
        var total = await query.CountAsync(cancellationToken);
        var pending = await query.CountAsync(pp => pp.Status == PickPackStatus.Pending, cancellationToken);
        var picking = await query.CountAsync(pp => pp.Status == PickPackStatus.Picking, cancellationToken);
        var picked = await query.CountAsync(pp => pp.Status == PickPackStatus.Picked, cancellationToken);
        var packing = await query.CountAsync(pp => pp.Status == PickPackStatus.Packing, cancellationToken);
        var packed = await query.CountAsync(pp => pp.Status == PickPackStatus.Packed, cancellationToken);
        var shipped = await query.CountAsync(pp => pp.Status == PickPackStatus.Shipped, cancellationToken);
        var cancelled = await query.CountAsync(pp => pp.Status == PickPackStatus.Cancelled, cancellationToken);

        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece Dictionary oluşturma)
        return new Dictionary<string, int>
        {
            { "Total", total },
            { "Pending", pending },
            { "Picking", picking },
            { "Picked", picked },
            { "Packing", packing },
            { "Packed", packed },
            { "Shipped", shipped },
            { "Cancelled", cancelled }
        };
    }
}

