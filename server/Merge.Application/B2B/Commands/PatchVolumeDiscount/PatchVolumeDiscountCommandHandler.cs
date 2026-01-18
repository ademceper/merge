using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Payment;
using Merge.Application.B2B.Commands.UpdateVolumeDiscount;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.PatchVolumeDiscount;

public class PatchVolumeDiscountCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<PatchVolumeDiscountCommandHandler> logger) : IRequestHandler<PatchVolumeDiscountCommand, bool>
{
    public async Task<bool> Handle(PatchVolumeDiscountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching volume discount. VolumeDiscountId: {VolumeDiscountId}", request.Id);

        var volumeDiscount = await context.Set<VolumeDiscount>()
            .FirstOrDefaultAsync(vd => vd.Id == request.Id, cancellationToken);

        if (volumeDiscount is null)
        {
            logger.LogWarning("Volume discount not found. VolumeDiscountId: {VolumeDiscountId}", request.Id);
            return false;
        }

        var dto = new CreateVolumeDiscountDto
        {
            ProductId = request.PatchDto.ProductId ?? volumeDiscount.ProductId,
            CategoryId = request.PatchDto.CategoryId ?? volumeDiscount.CategoryId,
            OrganizationId = request.PatchDto.OrganizationId ?? volumeDiscount.OrganizationId,
            MinQuantity = request.PatchDto.MinQuantity ?? volumeDiscount.MinQuantity,
            MaxQuantity = request.PatchDto.MaxQuantity ?? volumeDiscount.MaxQuantity,
            DiscountPercentage = request.PatchDto.DiscountPercentage ?? volumeDiscount.DiscountPercentage,
            FixedDiscountAmount = request.PatchDto.FixedDiscountAmount ?? volumeDiscount.FixedDiscountAmount,
            IsActive = request.PatchDto.IsActive ?? volumeDiscount.IsActive,
            StartDate = request.PatchDto.StartDate ?? volumeDiscount.StartDate,
            EndDate = request.PatchDto.EndDate ?? volumeDiscount.EndDate
        };

        var updateCommand = new UpdateVolumeDiscountCommand(request.Id, dto);

        return await mediator.Send(updateCommand, cancellationToken);
    }
}
