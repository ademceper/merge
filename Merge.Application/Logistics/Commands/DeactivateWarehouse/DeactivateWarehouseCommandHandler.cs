using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Commands.DeactivateWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeactivateWarehouseCommandHandler : IRequestHandler<DeactivateWarehouseCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateWarehouseCommandHandler> _logger;

    public DeactivateWarehouseCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateWarehouseCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating warehouse. WarehouseId: {WarehouseId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var warehouse = await _context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
        {
            _logger.LogWarning("Warehouse not found. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        warehouse.Deactivate();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse deactivated successfully. WarehouseId: {WarehouseId}", request.Id);
        return Unit.Value;
    }
}

