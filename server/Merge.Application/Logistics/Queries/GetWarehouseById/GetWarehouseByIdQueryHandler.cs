using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetWarehouseById;

public class GetWarehouseByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetWarehouseByIdQueryHandler> logger) : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto?>
{

    public async Task<WarehouseDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting warehouse. WarehouseId: {WarehouseId}", request.Id);

        var warehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        return warehouse != null ? mapper.Map<WarehouseDto>(warehouse) : null;
    }
}

