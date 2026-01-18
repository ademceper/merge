using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.UpdatePickPackDetails;

public class UpdatePickPackDetailsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdatePickPackDetailsCommandHandler> logger) : IRequestHandler<UpdatePickPackDetailsCommand, Unit>
{

    public async Task<Unit> Handle(UpdatePickPackDetailsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating pick-pack details. PickPackId: {PickPackId}", request.PickPackId);

        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack is null)
        {
            logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        // Sadece details update (notes, weight, dimensions, packageCount)
        // Status transition'ları için ayrı command'lar kullanılmalı (StartPicking, CompletePicking, StartPacking, CompletePacking, Ship, Cancel)
        pickPack.UpdateDetails(
            request.Notes,
            request.Weight,
            request.Dimensions,
            request.PackageCount);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pick-pack details updated successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

