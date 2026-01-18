using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class LoyaltyRuleCreatedEventHandler(ILogger<LoyaltyRuleCreatedEventHandler> logger) : INotificationHandler<LoyaltyRuleCreatedEvent>
{
    public async Task Handle(LoyaltyRuleCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "LoyaltyRule created event received. RuleId: {RuleId}, Name: {Name}, Type: {Type}, PointsAwarded: {PointsAwarded}",
            notification.RuleId, notification.Name, notification.Type, notification.PointsAwarded);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
