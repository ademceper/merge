using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.UpdateVolumeDiscount;

public class UpdateVolumeDiscountCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateVolumeDiscountCommandHandler> logger) : IRequestHandler<UpdateVolumeDiscountCommand, bool>
{

    public async Task<bool> Handle(UpdateVolumeDiscountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating volume discount. VolumeDiscountId: {VolumeDiscountId}", request.Id);


        var discount = await context.Set<VolumeDiscount>()
            .FirstOrDefaultAsync(vd => vd.Id == request.Id, cancellationToken);

        if (discount == null)
        {
            logger.LogWarning("Volume discount not found with Id: {VolumeDiscountId}", request.Id);
            return false;
        }

        discount.UpdateQuantityRange(request.Dto.MinQuantity, request.Dto.MaxQuantity);
        discount.UpdateDiscount(request.Dto.DiscountPercentage, request.Dto.FixedDiscountAmount);
        discount.UpdateDates(request.Dto.StartDate, request.Dto.EndDate);
        if (request.Dto.IsActive)
            discount.Activate();
        else
            discount.Deactivate();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Volume discount updated successfully. VolumeDiscountId: {VolumeDiscountId}", request.Id);
        return true;
    }
}

