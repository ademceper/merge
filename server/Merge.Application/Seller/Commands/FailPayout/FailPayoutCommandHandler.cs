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

namespace Merge.Application.Seller.Commands.FailPayout;

public class FailPayoutCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<FailPayoutCommandHandler> logger) : IRequestHandler<FailPayoutCommand, bool>
{

    public async Task<bool> Handle(FailPayoutCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Failing payout. PayoutId: {PayoutId}, Reason: {Reason}",
            request.PayoutId, request.Reason);

        var payout = await context.Set<CommissionPayout>()
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        if (payout == null)
        {
            logger.LogWarning("Payout not found. PayoutId: {PayoutId}", request.PayoutId);
            return false;
        }

        payout.Fail(request.Reason);

        // Revert commissions back to approved using domain method
        foreach (var item in payout.Items)
        {
            if (item.Commission != null)
            {
                item.Commission.RevertToApproved();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payout failed. PayoutId: {PayoutId}, Reason: {Reason}",
            request.PayoutId, request.Reason);

        return true;
    }
}
