using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class LoyaltyTierCreatedEventHandler(ILogger<LoyaltyTierCreatedEventHandler> logger) : INotificationHandler<LoyaltyTierCreatedEvent>
{
    public async Task Handle(LoyaltyTierCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "LoyaltyTier created event received. TierId: {TierId}, Name: {Name}, Level: {Level}, MinimumPoints: {MinimumPoints}",
            notification.TierId, notification.Name, notification.Level, notification.MinimumPoints);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
