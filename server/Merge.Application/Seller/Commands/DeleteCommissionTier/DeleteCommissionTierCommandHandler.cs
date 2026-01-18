using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.DeleteCommissionTier;

public class DeleteCommissionTierCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteCommissionTierCommandHandler> logger) : IRequestHandler<DeleteCommissionTierCommand, bool>
{

    public async Task<bool> Handle(DeleteCommissionTierCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting commission tier. TierId: {TierId}", request.TierId);

        var tier = await context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == request.TierId, cancellationToken);

        if (tier == null)
        {
            logger.LogWarning("Commission tier not found. TierId: {TierId}", request.TierId);
            return false;
        }

        tier.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Commission tier deleted. TierId: {TierId}", request.TierId);

        return true;
    }
}
