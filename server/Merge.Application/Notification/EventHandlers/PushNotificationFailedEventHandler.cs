using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class PushNotificationFailedEventHandler(ILogger<PushNotificationFailedEventHandler> logger) : INotificationHandler<PushNotificationFailedEvent>
{

    public async Task Handle(PushNotificationFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "PushNotification failed event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}, ErrorMessage: {ErrorMessage}",
            notification.PushNotificationId, notification.UserId, notification.ErrorMessage);

        // TODO: İleride burada şunlar yapılabilir:
        // - Retry mechanism
        // - Alert to admins
        // - Analytics tracking (failure rate)
        // - Device token cleanup if invalid

        await Task.CompletedTask;
    }
}
