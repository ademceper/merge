using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// LoyaltyTier Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class LoyaltyTierCreatedEventHandler(ILogger<LoyaltyTierCreatedEventHandler> logger) : INotificationHandler<LoyaltyTierCreatedEvent>
{
    public async Task Handle(LoyaltyTierCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "LoyaltyTier created event received. TierId: {TierId}, Name: {Name}, Level: {Level}, MinimumPoints: {MinimumPoints}",
            notification.TierId, notification.Name, notification.Level, notification.MinimumPoints);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
