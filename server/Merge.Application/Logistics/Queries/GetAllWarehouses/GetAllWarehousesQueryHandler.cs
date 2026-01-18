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
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetAllWarehouses;

public class GetAllWarehousesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllWarehousesQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetAllWarehousesQuery, IEnumerable<WarehouseDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all warehouses. IncludeInactive: {IncludeInactive}", request.IncludeInactive);

        var query = context.Set<Warehouse>().AsNoTracking();

        if (!request.IncludeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        var warehouses = await query
            .OrderBy(w => w.Name)
            .Take(_shippingSettings.QueryLimits.MaxWarehouses)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }
}

