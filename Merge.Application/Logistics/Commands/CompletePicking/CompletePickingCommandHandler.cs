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

namespace Merge.Application.Logistics.Commands.CompletePicking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CompletePickingCommandHandler : IRequestHandler<CompletePickingCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletePickingCommandHandler> _logger;

    public CompletePickingCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CompletePickingCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(CompletePickingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Completing picking. PickPackId: {PickPackId}", request.PickPackId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            _logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        // ✅ PERFORMANCE: Database'de kontrol et (memory'de işlem YASAK)
        // Check if all items are picked
        var totalItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == request.PickPackId, cancellationToken);

        var pickedItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == request.PickPackId && i.IsPicked, cancellationToken);

        if (totalItems == 0 || pickedItems < totalItems)
        {
            _logger.LogWarning("Not all items are picked. PickPackId: {PickPackId}, TotalItems: {TotalItems}, PickedItems: {PickedItems}",
                request.PickPackId, totalItems, pickedItems);
            throw new BusinessException("Tüm kalemler seçilmemiş.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        pickPack.CompletePicking();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Picking completed successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

