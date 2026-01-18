using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class SubscriptionPlanUpdatedEventHandler(ILogger<SubscriptionPlanUpdatedEventHandler> logger) : INotificationHandler<SubscriptionPlanUpdatedEvent>
{

    public async Task Handle(SubscriptionPlanUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Subscription plan updated event received. PlanId: {PlanId}, Name: {Name}",
            notification.PlanId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (subscription plans cache)
            // - Analytics tracking (plan update metrics)
            // - Email bildirimi (subscribers'a plan değişikliği bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionPlanUpdatedEvent. PlanId: {PlanId}",
                notification.PlanId);
            throw;
        }
    }
}
