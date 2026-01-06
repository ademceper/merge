using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Analytics.Queries.GetStockByWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetStockByWarehouseQueryHandler : IRequestHandler<GetStockByWarehouseQuery, List<WarehouseStockDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetStockByWarehouseQueryHandler> _logger;

    public GetStockByWarehouseQueryHandler(
        IDbContext context,
        ILogger<GetStockByWarehouseQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<WarehouseStockDto>> Handle(GetStockByWarehouseQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching stock by warehouse");

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted check (Global Query Filter handles it)
        return await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .GroupBy(i => new { i.WarehouseId, i.Warehouse.Name })
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
            .Select(g => new WarehouseStockDto(
                g.Key.WarehouseId,
                g.Key.Name,
                g.Count(),
                g.Sum(i => i.Quantity),
                g.Sum(i => i.Product.Price * i.Quantity)
            ))
            .ToListAsync(cancellationToken);
    }
}

