using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByInventoryId;

public class GetStockMovementsByInventoryIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetStockMovementsByInventoryIdQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetStockMovementsByInventoryIdQuery, IEnumerable<StockMovementDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<StockMovementDto>> Handle(GetStockMovementsByInventoryIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting stock movements by inventory. InventoryId: {InventoryId}", request.InventoryId);

        var movements = await context.Set<StockMovement>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting (AsSplitQuery) - Cartesian explosion önleme
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.InventoryId == request.InventoryId)
            .OrderByDescending(sm => sm.CreatedAt)
            .Take(_shippingSettings.QueryLimits.MaxStockMovementsPerInventory)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }
}

