using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class SubscriptionPlanActivatedEventHandler(ILogger<SubscriptionPlanActivatedEventHandler> logger) : INotificationHandler<SubscriptionPlanActivatedEvent>
{

    public async Task Handle(SubscriptionPlanActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Subscription plan activated event received. PlanId: {PlanId}, Name: {Name}",
            notification.PlanId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (subscription plans cache)
            // - Analytics tracking (plan activation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionPlanActivatedEvent. PlanId: {PlanId}",
                notification.PlanId);
            throw;
        }
    }
}
