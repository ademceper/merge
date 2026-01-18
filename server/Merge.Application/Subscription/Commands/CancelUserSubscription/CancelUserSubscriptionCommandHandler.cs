using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.CancelUserSubscription;

public class CancelUserSubscriptionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<CancelUserSubscriptionCommandHandler> logger) : IRequestHandler<CancelUserSubscriptionCommand, bool>
{

    public async Task<bool> Handle(CancelUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling user subscription. SubscriptionId: {SubscriptionId}",
            request.SubscriptionId);

        var subscription = await context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.SubscriptionId);
        }

        subscription.Cancel(request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User subscription cancelled successfully. SubscriptionId: {SubscriptionId}",
            subscription.Id);

        return true;
    }
}
