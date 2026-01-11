using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// SubscriptionPlan Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SubscriptionPlanActivatedEventHandler : INotificationHandler<SubscriptionPlanActivatedEvent>
{
    private readonly ILogger<SubscriptionPlanActivatedEventHandler> _logger;

    public SubscriptionPlanActivatedEventHandler(ILogger<SubscriptionPlanActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SubscriptionPlanActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SubscriptionPlanActivatedEvent. PlanId: {PlanId}",
                notification.PlanId);
            throw;
        }
    }
}
