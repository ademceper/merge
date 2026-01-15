using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetInventoryAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetInventoryAnalyticsQueryHandler(
    IDbContext context,
    ILogger<GetInventoryAnalyticsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetInventoryAnalyticsQuery, InventoryAnalyticsDto>
{

    public async Task<InventoryAnalyticsDto> Handle(GetInventoryAnalyticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching inventory analytics");

        var totalProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalStock = await context.Set<ProductEntity>()
            .AsNoTracking()
            .SumAsync(p => p.StockQuantity, cancellationToken);

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var lowStock = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StockQuantity > 0 && p.StockQuantity < settings.Value.LowStockThreshold, cancellationToken);

        var outOfStock = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StockQuantity == 0, cancellationToken);

        var totalValue = await context.Set<ProductEntity>()
            .AsNoTracking()
            .SumAsync(p => p.Price * p.StockQuantity, cancellationToken);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var lowStockProducts = await GetLowStockProductsAsync(settings.Value.MaxQueryLimit, cancellationToken);
        var stockByWarehouse = await GetStockByWarehouseAsync(cancellationToken);
        
        return new InventoryAnalyticsDto(
            totalProducts,
            totalStock,
            lowStock,
            outOfStock,
            totalValue,
            stockByWarehouse,
            lowStockProducts,
            new List<StockMovementSummaryDto>() // RecentMovements - şimdilik boş
        );
    }

    private async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int limit, CancellationToken cancellationToken)
    {
        var threshold = settings.Value.DefaultLowStockThreshold;
        return await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.StockQuantity < threshold && p.StockQuantity > 0)
            .Select(p => new LowStockProductDto(
                p.Id,
                p.Name,
                p.SKU,
                p.StockQuantity,
                threshold,
                threshold * 2
            ))
            .OrderBy(p => p.CurrentStock)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<WarehouseStockDto>> GetStockByWarehouseAsync(CancellationToken cancellationToken)
    {
        return await context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .GroupBy(i => new { i.WarehouseId, i.Warehouse.Name })
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

