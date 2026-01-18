using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.DeleteVolumeDiscount;

public class DeleteVolumeDiscountCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteVolumeDiscountCommandHandler> logger) : IRequestHandler<DeleteVolumeDiscountCommand, bool>
{

    public async Task<bool> Handle(DeleteVolumeDiscountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting volume discount. VolumeDiscountId: {VolumeDiscountId}", request.Id);

        var discount = await context.Set<VolumeDiscount>()
            .FirstOrDefaultAsync(vd => vd.Id == request.Id, cancellationToken);

        if (discount == null)
        {
            logger.LogWarning("Volume discount not found with Id: {VolumeDiscountId}", request.Id);
            return false;
        }

        discount.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Volume discount deleted successfully. VolumeDiscountId: {VolumeDiscountId}", request.Id);
        return true;
    }
}

