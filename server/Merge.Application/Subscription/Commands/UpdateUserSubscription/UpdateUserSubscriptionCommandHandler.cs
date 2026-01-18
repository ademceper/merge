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

namespace Merge.Application.Subscription.Commands.UpdateUserSubscription;

public class UpdateUserSubscriptionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateUserSubscriptionCommandHandler> logger) : IRequestHandler<UpdateUserSubscriptionCommand, bool>
{

    public async Task<bool> Handle(UpdateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating user subscription. SubscriptionId: {SubscriptionId}", request.Id);

        var subscription = await context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new NotFoundException("Abonelik", request.Id);
        }

        if (request.AutoRenew.HasValue)
        {
            subscription.UpdateAutoRenew(request.AutoRenew.Value);
        }

        if (!string.IsNullOrEmpty(request.PaymentMethodId))
        {
            subscription.UpdatePaymentMethod(request.PaymentMethodId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User subscription updated successfully. SubscriptionId: {SubscriptionId}", subscription.Id);

        return true;
    }
}
