using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetStockByWarehouse;

public class GetStockByWarehouseQueryHandler(
    IDbContext context,
    ILogger<GetStockByWarehouseQueryHandler> logger) : IRequestHandler<GetStockByWarehouseQuery, List<WarehouseStockDto>>
{

    public async Task<List<WarehouseStockDto>> Handle(GetStockByWarehouseQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching stock by warehouse");

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

