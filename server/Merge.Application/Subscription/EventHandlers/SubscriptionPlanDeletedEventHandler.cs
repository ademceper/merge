using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class SubscriptionPlanDeletedEventHandler(ILogger<SubscriptionPlanDeletedEventHandler> logger) : INotificationHandler<SubscriptionPlanDeletedEvent>
{

    public async Task Handle(SubscriptionPlanDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Subscription plan deleted event received. PlanId: {PlanId}, Name: {Name}",
            notification.PlanId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (subscription plans cache)
            // - Analytics tracking (plan deleted metrics)
            // - Email bildirimi (admin'lere plan silindi bildirimi)
            // - External system integration (CRM, marketing tools)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionPlanDeletedEvent. PlanId: {PlanId}, Name: {Name}",
                notification.PlanId, notification.Name);
            throw;
        }
    }
}
