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

namespace Merge.Application.Logistics.Commands.DeactivateWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class DeactivateWarehouseCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeactivateWarehouseCommandHandler> logger) : IRequestHandler<DeactivateWarehouseCommand, Unit>
{

    public async Task<Unit> Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deactivating warehouse. WarehouseId: {WarehouseId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var warehouse = await context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
        {
            logger.LogWarning("Warehouse not found. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        warehouse.Deactivate();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Warehouse deactivated successfully. WarehouseId: {WarehouseId}", request.Id);
        return Unit.Value;
    }
}

