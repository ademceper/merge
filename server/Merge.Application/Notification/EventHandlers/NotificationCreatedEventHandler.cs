using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationCreatedEventHandler(ILogger<NotificationCreatedEventHandler> logger) : INotificationHandler<NotificationCreatedEvent>
{

    public async Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Notification created event received. NotificationId: {NotificationId}, UserId: {UserId}, Type: {Type}, Title: {Title}",
            notification.NotificationId, notification.UserId, notification.Type, notification.Title);

        // TODO: İleride burada şunlar yapılabilir:
        // - Real-time notification gönderimi (SignalR, WebSocket)
        // - Push notification gönderimi (FCM, APNS)
        // - Email notification gönderimi (eğer tercih edilmişse)
        // - SMS notification gönderimi (eğer tercih edilmişse)
        // - Analytics tracking
        // - Cache invalidation
        // - External system integration

        await Task.CompletedTask;
    }
}
