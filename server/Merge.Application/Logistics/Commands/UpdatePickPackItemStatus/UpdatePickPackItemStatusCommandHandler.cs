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

public class UpdatePickPackItemStatusCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdatePickPackItemStatusCommandHandler> logger) : IRequestHandler<UpdatePickPackItemStatusCommand, Unit>
{

    public async Task<Unit> Handle(UpdatePickPackItemStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating pick-pack item status. ItemId: {ItemId}, IsPicked: {IsPicked}, IsPacked: {IsPacked}",
            request.ItemId, request.IsPicked, request.IsPacked);

        var item = await context.Set<PickPackItem>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item is null)
        {
            logger.LogWarning("Pick-pack item not found. ItemId: {ItemId}", request.ItemId);
            throw new NotFoundException("Pick-pack kalemi", request.ItemId);
        }

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pick-pack item status updated successfully. ItemId: {ItemId}", request.ItemId);
        return Unit.Value;
    }
}

