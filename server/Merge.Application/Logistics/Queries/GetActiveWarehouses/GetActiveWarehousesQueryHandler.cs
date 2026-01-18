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

namespace Merge.Application.Logistics.Queries.GetActiveWarehouses;

public class GetActiveWarehousesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetActiveWarehousesQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetActiveWarehousesQuery, IEnumerable<WarehouseDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<WarehouseDto>> Handle(GetActiveWarehousesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting active warehouses");

        var warehouses = await context.Set<Warehouse>()
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .Take(_shippingSettings.QueryLimits.MaxWarehouses)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }
}

