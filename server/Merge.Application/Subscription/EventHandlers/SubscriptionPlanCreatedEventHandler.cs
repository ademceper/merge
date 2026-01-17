using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// SubscriptionPlan Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SubscriptionPlanCreatedEventHandler(ILogger<SubscriptionPlanCreatedEventHandler> logger) : INotificationHandler<SubscriptionPlanCreatedEvent>
{

    public async Task Handle(SubscriptionPlanCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Subscription plan created event received. PlanId: {PlanId}, Name: {Name}, PlanType: {PlanType}, Price: {Price}, Currency: {Currency}",
            notification.PlanId, notification.Name, notification.PlanType, notification.Price, notification.Currency);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (subscription plans cache)
            // - Analytics tracking (new plan created metrics)
            // - Email bildirimi (admin'lere yeni plan oluşturuldu bildirimi)
            // - External system integration (CRM, marketing tools)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling SubscriptionPlanCreatedEvent. PlanId: {PlanId}, Name: {Name}",
                notification.PlanId, notification.Name);
            throw;
        }
    }
}
