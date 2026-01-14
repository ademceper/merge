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

namespace Merge.Application.Logistics.Commands.CompletePacking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CompletePackingCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CompletePackingCommandHandler> logger) : IRequestHandler<CompletePackingCommand, Unit>
{

    public async Task<Unit> Handle(CompletePackingCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Completing packing. PickPackId: {PickPackId}, Weight: {Weight}, PackageCount: {PackageCount}",
            request.PickPackId, request.Weight, request.PackageCount);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        // ✅ PERFORMANCE: Database'de kontrol et (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Tek sorguda GroupBy ile total ve packed item sayılarını al (2 ayrı CountAsync yerine)
        var itemCounts = await context.Set<PickPackItem>()
            .AsNoTracking()
            .Where(i => i.PickPackId == request.PickPackId)
            .GroupBy(i => 1)
            .Select(g => new
            {
                TotalItems = g.Count(),
                PackedItems = g.Count(i => i.IsPacked)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (itemCounts == null || itemCounts.TotalItems == 0 || itemCounts.PackedItems < itemCounts.TotalItems)
        {
            logger.LogWarning("Not all items are packed. PickPackId: {PickPackId}, TotalItems: {TotalItems}, PackedItems: {PackedItems}",
                request.PickPackId, itemCounts?.TotalItems ?? 0, itemCounts?.PackedItems ?? 0);
            throw new BusinessException("Tüm kalemler paketlenmemiş.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        pickPack.CompletePacking(request.Weight, request.Dimensions, request.PackageCount);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Packing completed successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

