using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.PatchCommissionTier;

/// <summary>
/// Handler for PatchCommissionTierCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchCommissionTierCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<PatchCommissionTierCommandHandler> logger) : IRequestHandler<PatchCommissionTierCommand, bool>
{
    public async Task<bool> Handle(PatchCommissionTierCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching commission tier. TierId: {TierId}", request.TierId);

        var tier = await context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == request.TierId, cancellationToken);

        if (tier is null)
        {
            logger.LogWarning("Commission tier not found. TierId: {TierId}", request.TierId);
            return false;
        }

        // Apply partial updates - get existing values if not provided
        var name = request.PatchDto.Name ?? tier.Name;
        var minSales = request.PatchDto.MinSales ?? tier.MinSales;
        var maxSales = request.PatchDto.MaxSales ?? tier.MaxSales;
        var commissionRate = request.PatchDto.CommissionRate ?? tier.CommissionRate;
        var platformFeeRate = request.PatchDto.PlatformFeeRate ?? tier.PlatformFeeRate;
        var priority = request.PatchDto.Priority ?? tier.Priority;

        tier.UpdateDetails(name, minSales, maxSales, commissionRate, platformFeeRate, priority);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Commission tier patched. TierId: {TierId}", request.TierId);

        return true;
    }
}
