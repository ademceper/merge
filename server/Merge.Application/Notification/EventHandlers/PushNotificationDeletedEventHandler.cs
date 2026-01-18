using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class PushNotificationDeletedEventHandler(ILogger<PushNotificationDeletedEventHandler> logger) : INotificationHandler<PushNotificationDeletedEvent>
{

    public async Task Handle(PushNotificationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "PushNotification deleted event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}",
            notification.PushNotificationId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (deletion rate)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
