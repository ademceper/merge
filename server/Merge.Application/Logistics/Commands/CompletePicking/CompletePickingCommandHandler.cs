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
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CompletePickingCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CompletePickingCommandHandler> logger) : IRequestHandler<CompletePickingCommand, Unit>
{

    public async Task<Unit> Handle(CompletePickingCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Completing picking. PickPackId: {PickPackId}", request.PickPackId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        // ✅ PERFORMANCE: Database'de kontrol et (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Tek sorguda GroupBy ile total ve picked item sayılarını al (2 ayrı CountAsync yerine)
        var itemCounts = await context.Set<PickPackItem>()
            .AsNoTracking()
            .Where(i => i.PickPackId == request.PickPackId)
            .GroupBy(i => 1)
            .Select(g => new
            {
                TotalItems = g.Count(),
                PickedItems = g.Count(i => i.IsPicked)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (itemCounts == null || itemCounts.TotalItems == 0 || itemCounts.PickedItems < itemCounts.TotalItems)
        {
            logger.LogWarning("Not all items are picked. PickPackId: {PickPackId}, TotalItems: {TotalItems}, PickedItems: {PickedItems}",
                request.PickPackId, itemCounts?.TotalItems ?? 0, itemCounts?.PickedItems ?? 0);
            throw new BusinessException("Tüm kalemler seçilmemiş.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        pickPack.CompletePicking();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Picking completed successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

