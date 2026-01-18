using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class SubscriptionPlanDeactivatedEventHandler(ILogger<SubscriptionPlanDeactivatedEventHandler> logger) : INotificationHandler<SubscriptionPlanDeactivatedEvent>
{

    public async Task Handle(SubscriptionPlanDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Subscription plan deactivated event received. PlanId: {PlanId}, Name: {Name}",
            notification.PlanId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (subscription plans cache)
            // - Analytics tracking (plan deactivation metrics)
            // - Email bildirimi (subscribers'a plan devre dışı bırakıldı bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionPlanDeactivatedEvent. PlanId: {PlanId}",
                notification.PlanId);
            throw;
        }
    }
}
