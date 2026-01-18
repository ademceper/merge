using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.UpdateCommissionTier;

public class UpdateCommissionTierCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateCommissionTierCommandHandler> logger) : IRequestHandler<UpdateCommissionTierCommand, bool>
{

    public async Task<bool> Handle(UpdateCommissionTierCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating commission tier. TierId: {TierId}", request.TierId);

        var tier = await context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == request.TierId, cancellationToken);

        if (tier is null)
        {
            logger.LogWarning("Commission tier not found. TierId: {TierId}", request.TierId);
            return false;
        }

        tier.UpdateDetails(
            name: request.Name,
            minSales: request.MinSales,
            maxSales: request.MaxSales,
            commissionRate: request.CommissionRate,
            platformFeeRate: request.PlatformFeeRate,
            priority: request.Priority);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Commission tier updated. TierId: {TierId}", request.TierId);

        return true;
    }
}
