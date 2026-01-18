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

namespace Merge.Application.Subscription.Commands.ActivateSubscription;

public class ActivateSubscriptionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ActivateSubscriptionCommandHandler> logger) : IRequestHandler<ActivateSubscriptionCommand, bool>
{

    public async Task<bool> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Activating subscription. SubscriptionId: {SubscriptionId}", request.SubscriptionId);

        var subscription = await context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.SubscriptionId);
        }

        subscription.Activate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription activated successfully. SubscriptionId: {SubscriptionId}", subscription.Id);

        return true;
    }
}
