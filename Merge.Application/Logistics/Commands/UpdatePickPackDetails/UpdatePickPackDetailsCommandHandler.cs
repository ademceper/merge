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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class UpdatePickPackDetailsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdatePickPackDetailsCommandHandler> logger) : IRequestHandler<UpdatePickPackDetailsCommand, Unit>
{

    public async Task<Unit> Handle(UpdatePickPackDetailsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating pick-pack details. PickPackId: {PickPackId}", request.PickPackId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Sadece details update (notes, weight, dimensions, packageCount)
        // Status transition'ları için ayrı command'lar kullanılmalı (StartPicking, CompletePicking, StartPacking, CompletePacking, Ship, Cancel)
        pickPack.UpdateDetails(
            request.Notes,
            request.Weight,
            request.Dimensions,
            request.PackageCount);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pick-pack details updated successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

