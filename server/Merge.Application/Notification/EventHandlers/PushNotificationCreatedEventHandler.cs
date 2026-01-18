using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class PushNotificationCreatedEventHandler(ILogger<PushNotificationCreatedEventHandler> logger) : INotificationHandler<PushNotificationCreatedEvent>
{

    public async Task Handle(PushNotificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "PushNotification created event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}, DeviceId: {DeviceId}, NotificationType: {NotificationType}, Title: {Title}",
            notification.PushNotificationId, notification.UserId, notification.DeviceId, notification.NotificationType, notification.Title);

        // TODO: İleride burada şunlar yapılabilir:
        // - Send push notification via FCM/APNS
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
