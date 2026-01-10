using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Commands.DeleteWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteWarehouseCommandHandler> _logger;

    public DeleteWarehouseCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteWarehouseCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting warehouse. WarehouseId: {WarehouseId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var warehouse = await _context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
        {
            _logger.LogWarning("Warehouse not found for deletion. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        // ✅ PERFORMANCE: AsNoTracking - Check if warehouse has inventory
        var hasInventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .AnyAsync(i => i.WarehouseId == request.Id, cancellationToken);

        if (hasInventory)
        {
            _logger.LogWarning("Warehouse has inventory, cannot delete. WarehouseId: {WarehouseId}", request.Id);
            throw new BusinessException("Envanteri olan bir depo silinemez. Önce envanteri transfer edin veya kaldırın.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        warehouse.MarkAsDeleted();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse deleted successfully. WarehouseId: {WarehouseId}", request.Id);
        return Unit.Value;
    }
}

