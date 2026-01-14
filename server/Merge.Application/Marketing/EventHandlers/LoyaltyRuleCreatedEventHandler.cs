using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// LoyaltyRule Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class LoyaltyRuleCreatedEventHandler(ILogger<LoyaltyRuleCreatedEventHandler> logger) : INotificationHandler<LoyaltyRuleCreatedEvent>
{
    public async Task Handle(LoyaltyRuleCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "LoyaltyRule created event received. RuleId: {RuleId}, Name: {Name}, Type: {Type}, PointsAwarded: {PointsAwarded}",
            notification.RuleId, notification.Name, notification.Type, notification.PointsAwarded);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
