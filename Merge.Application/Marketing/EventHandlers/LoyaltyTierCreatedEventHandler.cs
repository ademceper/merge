using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// LoyaltyTier Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class LoyaltyTierCreatedEventHandler : INotificationHandler<LoyaltyTierCreatedEvent>
{
    private readonly ILogger<LoyaltyTierCreatedEventHandler> _logger;

    public LoyaltyTierCreatedEventHandler(ILogger<LoyaltyTierCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LoyaltyTierCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "LoyaltyTier created event received. TierId: {TierId}, Name: {Name}, Level: {Level}, MinimumPoints: {MinimumPoints}",
            notification.TierId, notification.Name, notification.Level, notification.MinimumPoints);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
