using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetInventoryOverview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetInventoryOverviewQueryHandler(
    IDbContext context,
    ILogger<GetInventoryOverviewQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IMapper mapper) : IRequestHandler<GetInventoryOverviewQuery, InventoryOverviewDto>
{

    public async Task<InventoryOverviewDto> Handle(GetInventoryOverviewQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching inventory overview");
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted checks (Global Query Filter handles it)
        var totalInventoryValue = await context.Set<Inventory>()
            .AsNoTracking()
            .SumAsync(i => i.Quantity * i.UnitCost, cancellationToken);

        // ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
        // ✅ PERFORMANCE: Database'de filtreleme yap (memory'de değil)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var maxAlerts = settings.Value.MaxLowStockAlertsInOverview;
        var lowStockInventories = await context.Set<Inventory>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.Quantity <= i.MinimumStockLevel)
            .OrderBy(i => i.Quantity)
            .Take(maxAlerts)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var lowStockAlerts = mapper.Map<List<LowStockAlertDto>>(lowStockInventories);

        var overview = new InventoryOverviewDto(
            TotalWarehouses: await context.Set<Warehouse>().AsNoTracking().CountAsync(w => w.IsActive, cancellationToken),
            TotalInventoryItems: await context.Set<Inventory>().AsNoTracking().CountAsync(cancellationToken),
            TotalInventoryValue: totalInventoryValue,
            LowStockCount: await context.Set<Inventory>().AsNoTracking().CountAsync(i => i.Quantity <= i.MinimumStockLevel, cancellationToken),
            LowStockAlerts: lowStockAlerts,
            TotalStockQuantity: await context.Set<Inventory>().AsNoTracking().SumAsync(i => i.Quantity, cancellationToken),
            ReservedStockQuantity: await context.Set<Inventory>().AsNoTracking().SumAsync(i => i.ReservedQuantity, cancellationToken)
        );

        logger.LogInformation("Inventory overview calculated. TotalWarehouses: {TotalWarehouses}, TotalInventoryValue: {TotalInventoryValue}",
            overview.TotalWarehouses, overview.TotalInventoryValue);

        return overview;
    }
}

