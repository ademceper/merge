using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByProductId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetStockMovementsByProductIdQueryHandler : IRequestHandler<GetStockMovementsByProductIdQuery, PagedResult<StockMovementDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStockMovementsByProductIdQueryHandler> _logger;

    public GetStockMovementsByProductIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetStockMovementsByProductIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<StockMovementDto>> Handle(GetStockMovementsByProductIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stock movements by product. ProductId: {ProductId}, Page: {Page}, PageSize: {PageSize}", request.ProductId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize; // Max limit

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var query = _context.Set<StockMovement>()
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.ProductId == request.ProductId);

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

