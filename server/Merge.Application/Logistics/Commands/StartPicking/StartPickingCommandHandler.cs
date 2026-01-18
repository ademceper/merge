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

namespace Merge.Application.Logistics.Commands.StartPicking;

public class StartPickingCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<StartPickingCommandHandler> logger) : IRequestHandler<StartPickingCommand, Unit>
{

    public async Task<Unit> Handle(StartPickingCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting picking. PickPackId: {PickPackId}, UserId: {UserId}", request.PickPackId, request.UserId);

        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        pickPack.StartPicking(request.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Picking started successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

