using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetFilteredStockMovements;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetFilteredStockMovementsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetFilteredStockMovementsQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetFilteredStockMovementsQuery, IEnumerable<StockMovementDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<StockMovementDto>> Handle(GetFilteredStockMovementsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting filtered stock movements. ProductId: {ProductId}, WarehouseId: {WarehouseId}, MovementType: {MovementType}",
            request.ProductId, request.WarehouseId, request.MovementType);

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _shippingSettings.QueryLimits.MaxPageSize 
            ? _shippingSettings.QueryLimits.MaxPageSize 
            : request.PageSize;

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için cartesian explosion önleme
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        IQueryable<StockMovement> query = context.Set<StockMovement>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting (AsSplitQuery) - Cartesian explosion önleme
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse);

        if (request.ProductId.HasValue)
        {
            query = query.Where(sm => sm.ProductId == request.ProductId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(sm => sm.WarehouseId == request.WarehouseId.Value);
        }

        if (request.MovementType.HasValue)
        {
            query = query.Where(sm => sm.MovementType == request.MovementType.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(sm => sm.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(sm => sm.CreatedAt <= request.EndDate.Value);
        }

        var movements = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }
}

