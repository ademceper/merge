using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.ActivateWarehouse;

public class ActivateWarehouseCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ActivateWarehouseCommandHandler> logger) : IRequestHandler<ActivateWarehouseCommand, Unit>
{

    public async Task<Unit> Handle(ActivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Activating warehouse. WarehouseId: {WarehouseId}", request.Id);

        var warehouse = await context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
        {
            logger.LogWarning("Warehouse not found. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        warehouse.Activate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Warehouse activated successfully. WarehouseId: {WarehouseId}", request.Id);
        return Unit.Value;
    }
}

