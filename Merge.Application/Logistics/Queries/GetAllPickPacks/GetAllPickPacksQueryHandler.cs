using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetAllPickPacks;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetAllPickPacksQueryHandler : IRequestHandler<GetAllPickPacksQuery, PagedResult<PickPackDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllPickPacksQueryHandler> _logger;

    public GetAllPickPacksQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllPickPacksQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<PickPackDto>> Handle(GetAllPickPacksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all pick-packs. Status: {Status}, WarehouseId: {WarehouseId}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.WarehouseId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize; // Max limit

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        IQueryable<PickPack> query = _context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product);

        if (request.Status.HasValue)
        {
            query = query.Where(pp => pp.Status == request.Status.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(pp => pp.WarehouseId == request.WarehouseId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pickPacks = await query
            .OrderByDescending(pp => pp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        var items = _mapper.Map<IEnumerable<PickPackDto>>(pickPacks);

        return new PagedResult<PickPackDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

