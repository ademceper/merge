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
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetPickPacksByOrderId;

public class GetPickPacksByOrderIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetPickPacksByOrderIdQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetPickPacksByOrderIdQuery, IEnumerable<PickPackDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<PickPackDto>> Handle(GetPickPacksByOrderIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting pick-packs by order. OrderId: {OrderId}", request.OrderId);

        var pickPacks = await context.Set<PickPack>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting (AsSplitQuery) - Cartesian explosion önleme
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .Where(pp => pp.OrderId == request.OrderId)
            .OrderByDescending(pp => pp.CreatedAt)
            .Take(_shippingSettings.QueryLimits.MaxPickPacksPerOrder)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<PickPackDto>>(pickPacks);
    }
}

