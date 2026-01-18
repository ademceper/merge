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

namespace Merge.Application.Subscription.Commands.RenewSubscription;

public class RenewSubscriptionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RenewSubscriptionCommandHandler> logger) : IRequestHandler<RenewSubscriptionCommand, bool>
{

    public async Task<bool> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Renewing subscription. SubscriptionId: {SubscriptionId}", request.SubscriptionId);

        var subscription = await context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new NotFoundException("Abonelik", request.SubscriptionId);
        }

        var plan = subscription.SubscriptionPlan;
        if (plan is null)
        {
            throw new NotFoundException("Abonelik planı", subscription.SubscriptionPlanId);
        }

        subscription.Renew(plan);

        // Create payment for renewal
        var billingPeriodStart = subscription.NextBillingDate ?? subscription.EndDate.AddDays(-plan.DurationDays);
        var billingPeriodEnd = subscription.EndDate;
        
        var payment = SubscriptionPayment.Create(
            subscription: subscription,
            amount: plan.Price,
            billingPeriodStart: billingPeriodStart,
            billingPeriodEnd: billingPeriodEnd);

        await context.Set<SubscriptionPayment>().AddAsync(payment, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription renewed successfully. SubscriptionId: {SubscriptionId}, RenewalCount: {RenewalCount}",
            subscription.Id, subscription.RenewalCount);

        return true;
    }
}
