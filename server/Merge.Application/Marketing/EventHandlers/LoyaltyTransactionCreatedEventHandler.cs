using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class LoyaltyTransactionCreatedEventHandler(ILogger<LoyaltyTransactionCreatedEventHandler> logger) : INotificationHandler<LoyaltyTransactionCreatedEvent>
{
    public async Task Handle(LoyaltyTransactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "LoyaltyTransaction created event received. TransactionId: {TransactionId}, UserId: {UserId}, Points: {Points}, Type: {Type}",
            notification.TransactionId, notification.UserId, notification.Points, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Tier upgrade kontrolü

        await Task.CompletedTask;
    }
}
