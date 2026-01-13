using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// LoyaltyTransaction Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class LoyaltyTransactionCreatedEventHandler(ILogger<LoyaltyTransactionCreatedEventHandler> logger) : INotificationHandler<LoyaltyTransactionCreatedEvent>
{
    public async Task Handle(LoyaltyTransactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "LoyaltyTransaction created event received. TransactionId: {TransactionId}, UserId: {UserId}, Points: {Points}, Type: {Type}",
            notification.TransactionId, notification.UserId, notification.Points, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Tier upgrade kontrolü

        await Task.CompletedTask;
    }
}
