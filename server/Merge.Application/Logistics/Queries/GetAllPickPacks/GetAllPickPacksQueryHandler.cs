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
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetAllPickPacks;

public class GetAllPickPacksQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllPickPacksQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetAllPickPacksQuery, PagedResult<PickPackDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<PagedResult<PickPackDto>> Handle(GetAllPickPacksQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all pick-packs. Status: {Status}, WarehouseId: {WarehouseId}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.WarehouseId, request.Page, request.PageSize);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _shippingSettings.QueryLimits.MaxPageSize 
            ? _shippingSettings.QueryLimits.MaxPageSize 
            : request.PageSize;

        IQueryable<PickPack> query = context.Set<PickPack>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting (AsSplitQuery) - Cartesian explosion önleme
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

        var items = mapper.Map<IEnumerable<PickPackDto>>(pickPacks);

        return new PagedResult<PickPackDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

