using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class TierUpdatedEventHandler(ILogger<TierUpdatedEventHandler> logger) : INotificationHandler<TierUpdatedEvent>
{
    public async Task Handle(TierUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Tier updated event received. AccountId: {AccountId}, UserId: {UserId}, NewTierId: {NewTierId}",
            notification.AccountId, notification.UserId, notification.NewTierId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Tier upgrade email gönderimi
        // - Benefits notification
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
