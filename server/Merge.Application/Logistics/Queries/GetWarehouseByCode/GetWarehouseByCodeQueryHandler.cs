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

namespace Merge.Application.Logistics.Queries.GetWarehouseByCode;

public class GetWarehouseByCodeQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetWarehouseByCodeQueryHandler> logger) : IRequestHandler<GetWarehouseByCodeQuery, WarehouseDto?>
{

    public async Task<WarehouseDto?> Handle(GetWarehouseByCodeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting warehouse by code. Code: {Code}", request.Code);

        var warehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == request.Code, cancellationToken);

        return warehouse is not null ? mapper.Map<WarehouseDto>(warehouse) : null;
    }
}

