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

namespace Merge.Application.Seller.Commands.ProcessPayout;

public class ProcessPayoutCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ProcessPayoutCommandHandler> logger) : IRequestHandler<ProcessPayoutCommand, bool>
{

    public async Task<bool> Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing payout. PayoutId: {PayoutId}, TransactionReference: {TransactionReference}",
            request.PayoutId, request.TransactionReference);

        var payout = await context.Set<CommissionPayout>()
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        if (payout is null)
        {
            logger.LogWarning("Payout not found. PayoutId: {PayoutId}", request.PayoutId);
            return false;
        }

        payout.Process(request.TransactionReference);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payout processed. PayoutId: {PayoutId}", request.PayoutId);

        return true;
    }
}
