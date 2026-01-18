using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.ApproveCommission;

public class ApproveCommissionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ApproveCommissionCommandHandler> logger) : IRequestHandler<ApproveCommissionCommand, bool>
{

    public async Task<bool> Handle(ApproveCommissionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving commission. CommissionId: {CommissionId}", request.CommissionId);

        var commission = await context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == request.CommissionId, cancellationToken);

        if (commission == null)
        {
            logger.LogWarning("Commission not found. CommissionId: {CommissionId}", request.CommissionId);
            return false;
        }

        commission.Approve();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Commission approved. CommissionId: {CommissionId}", request.CommissionId);

        return true;
    }
}
