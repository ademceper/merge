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

namespace Merge.Application.Logistics.Commands.UpdatePickPackItemStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class UpdatePickPackItemStatusCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdatePickPackItemStatusCommandHandler> logger) : IRequestHandler<UpdatePickPackItemStatusCommand, Unit>
{

    public async Task<Unit> Handle(UpdatePickPackItemStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating pick-pack item status. ItemId: {ItemId}, IsPicked: {IsPicked}, IsPacked: {IsPacked}",
            request.ItemId, request.IsPicked, request.IsPacked);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var item = await context.Set<PickPackItem>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
        {
            logger.LogWarning("Pick-pack item not found. ItemId: {ItemId}", request.ItemId);
            throw new NotFoundException("Pick-pack kalemi", request.ItemId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.IsPicked.HasValue && request.IsPicked.Value && !item.IsPicked)
        {
            item.MarkAsPicked();
        }

        if (request.IsPacked.HasValue && request.IsPacked.Value && !item.IsPacked)
        {
            item.MarkAsPacked();
        }

        if (!string.IsNullOrEmpty(request.Location))
        {
            item.UpdateLocation(request.Location);
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pick-pack item status updated successfully. ItemId: {ItemId}", request.ItemId);
        return Unit.Value;
    }
}

