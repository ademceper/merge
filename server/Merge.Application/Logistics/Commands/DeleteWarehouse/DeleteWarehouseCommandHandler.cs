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

namespace Merge.Application.Logistics.Commands.DeleteWarehouse;

public class DeleteWarehouseCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteWarehouseCommandHandler> logger) : IRequestHandler<DeleteWarehouseCommand, Unit>
{

    public async Task<Unit> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting warehouse. WarehouseId: {WarehouseId}", request.Id);

        var warehouse = await context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
        {
            logger.LogWarning("Warehouse not found for deletion. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        var hasInventory = await context.Set<Inventory>()
            .AsNoTracking()
            .AnyAsync(i => i.WarehouseId == request.Id, cancellationToken);

        if (hasInventory)
        {
            logger.LogWarning("Warehouse has inventory, cannot delete. WarehouseId: {WarehouseId}", request.Id);
            throw new BusinessException("Envanteri olan bir depo silinemez. Önce envanteri transfer edin veya kaldırın.");
        }

        warehouse.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Warehouse deleted successfully. WarehouseId: {WarehouseId}", request.Id);
        return Unit.Value;
    }
}

