using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByWarehouseId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetStockMovementsByWarehouseIdQueryHandler : IRequestHandler<GetStockMovementsByWarehouseIdQuery, PagedResult<StockMovementDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStockMovementsByWarehouseIdQueryHandler> _logger;
    private readonly ShippingSettings _shippingSettings;

    public GetStockMovementsByWarehouseIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetStockMovementsByWarehouseIdQueryHandler> logger,
        IOptions<ShippingSettings> shippingSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _shippingSettings = shippingSettings.Value;
    }

    public async Task<PagedResult<StockMovementDto>> Handle(GetStockMovementsByWarehouseIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stock movements by warehouse. WarehouseId: {WarehouseId}, Page: {Page}, PageSize: {PageSize}", request.WarehouseId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _shippingSettings.QueryLimits.MaxPageSize 
            ? _shippingSettings.QueryLimits.MaxPageSize 
            : request.PageSize;

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için cartesian explosion önleme
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var query = _context.Set<StockMovement>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting (AsSplitQuery) - Cartesian explosion önleme
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.WarehouseId == request.WarehouseId);

        var totalCount = await query.CountAsync(cancellationToken);

        var movements = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        var items = _mapper.Map<IEnumerable<StockMovementDto>>(movements);

        return new PagedResult<StockMovementDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

