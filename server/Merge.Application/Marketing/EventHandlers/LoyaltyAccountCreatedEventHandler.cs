using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class LoyaltyAccountCreatedEventHandler(ILogger<LoyaltyAccountCreatedEventHandler> logger) : INotificationHandler<LoyaltyAccountCreatedEvent>
{
    public async Task Handle(LoyaltyAccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Loyalty account created event received. AccountId: {AccountId}, UserId: {UserId}",
            notification.AccountId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi
        // - Analytics tracking
        // - Initial tier assignment

        await Task.CompletedTask;
    }
}
