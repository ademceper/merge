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

public class CompletePackingCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CompletePackingCommandHandler> logger) : IRequestHandler<CompletePackingCommand, Unit>
{

    public async Task<Unit> Handle(CompletePackingCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Completing packing. PickPackId: {PickPackId}, Weight: {Weight}, PackageCount: {PackageCount}",
            request.PickPackId, request.Weight, request.PackageCount);

        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack is null)
        {
            logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

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

        if (itemCounts is null || itemCounts.TotalItems == 0 || itemCounts.PackedItems < itemCounts.TotalItems)
        {
            logger.LogWarning("Not all items are packed. PickPackId: {PickPackId}, TotalItems: {TotalItems}, PackedItems: {PackedItems}",
                request.PickPackId, itemCounts?.TotalItems ?? 0, itemCounts?.PackedItems ?? 0);
            throw new BusinessException("Tüm kalemler paketlenmemiş.");
        }

        pickPack.CompletePacking(request.Weight, request.Dimensions, request.PackageCount);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Packing completed successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

