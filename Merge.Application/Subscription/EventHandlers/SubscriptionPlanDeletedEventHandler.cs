using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// SubscriptionPlan Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SubscriptionPlanDeletedEventHandler : INotificationHandler<SubscriptionPlanDeletedEvent>
{
    private readonly ILogger<SubscriptionPlanDeletedEventHandler> _logger;

    public SubscriptionPlanDeletedEventHandler(ILogger<SubscriptionPlanDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SubscriptionPlanDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SubscriptionPlanDeletedEvent. PlanId: {PlanId}, Name: {Name}",
                notification.PlanId, notification.Name);
            throw;
        }
    }
}
