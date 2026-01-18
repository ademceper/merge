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

namespace Merge.Application.Subscription.Commands.SuspendSubscription;

public class SuspendSubscriptionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SuspendSubscriptionCommandHandler> logger) : IRequestHandler<SuspendSubscriptionCommand, bool>
{

    public async Task<bool> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Suspending subscription. SubscriptionId: {SubscriptionId}", request.SubscriptionId);

        var subscription = await context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new NotFoundException("Abonelik", request.SubscriptionId);
        }

        subscription.Suspend();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription suspended successfully. SubscriptionId: {SubscriptionId}", subscription.Id);

        return true;
    }
}
