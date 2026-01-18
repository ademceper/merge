using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetStockMovementById;

public class GetStockMovementByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetStockMovementByIdQueryHandler> logger) : IRequestHandler<GetStockMovementByIdQuery, StockMovementDto?>
{

    public async Task<StockMovementDto?> Handle(GetStockMovementByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting stock movement. StockMovementId: {StockMovementId}", request.Id);

        var movement = await context.Set<StockMovement>()
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .FirstOrDefaultAsync(sm => sm.Id == request.Id, cancellationToken);

        return movement is not null ? mapper.Map<StockMovementDto>(movement) : null;
    }
}

