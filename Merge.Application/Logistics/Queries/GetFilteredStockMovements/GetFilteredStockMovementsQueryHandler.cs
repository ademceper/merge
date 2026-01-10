using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Queries.GetFilteredStockMovements;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetFilteredStockMovementsQueryHandler : IRequestHandler<GetFilteredStockMovementsQuery, IEnumerable<StockMovementDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFilteredStockMovementsQueryHandler> _logger;

    public GetFilteredStockMovementsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetFilteredStockMovementsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<StockMovementDto>> Handle(GetFilteredStockMovementsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting filtered stock movements. ProductId: {ProductId}, WarehouseId: {WarehouseId}, MovementType: {MovementType}",
            request.ProductId, request.WarehouseId, request.MovementType);

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize; // Max limit

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        IQueryable<StockMovement> query = _context.Set<StockMovement>()
            .AsNoTracking()
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
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }
}

