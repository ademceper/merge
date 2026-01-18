using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class PushNotificationBouncedEventHandler(ILogger<PushNotificationBouncedEventHandler> logger) : INotificationHandler<PushNotificationBouncedEvent>
{

    public async Task Handle(PushNotificationBouncedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "PushNotification bounced event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}",
            notification.PushNotificationId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Device token cleanup
        // - Analytics tracking (bounce rate)
        // - User notification preference update

        await Task.CompletedTask;
    }
}
