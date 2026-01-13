using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// LoyaltyTransaction Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class LoyaltyTransactionCreatedEventHandler : INotificationHandler<LoyaltyTransactionCreatedEvent>
{
    private readonly ILogger<LoyaltyTransactionCreatedEventHandler> _logger;

    public LoyaltyTransactionCreatedEventHandler(ILogger<LoyaltyTransactionCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LoyaltyTransactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "LoyaltyTransaction created event received. TransactionId: {TransactionId}, UserId: {UserId}, Points: {Points}, Type: {Type}",
            notification.TransactionId, notification.UserId, notification.Points, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Tier upgrade kontrolü

        await Task.CompletedTask;
    }
}
