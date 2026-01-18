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

namespace Merge.Application.Logistics.Commands.MarkPickPackAsShipped;

public class MarkPickPackAsShippedCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<MarkPickPackAsShippedCommandHandler> logger) : IRequestHandler<MarkPickPackAsShippedCommand, Unit>
{

    public async Task<Unit> Handle(MarkPickPackAsShippedCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Marking pick-pack as shipped. PickPackId: {PickPackId}", request.PickPackId);

        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        pickPack.Ship();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pick-pack marked as shipped successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

