using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// PushNotification Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class PushNotificationCreatedEventHandler : INotificationHandler<PushNotificationCreatedEvent>
{
    private readonly ILogger<PushNotificationCreatedEventHandler> _logger;

    public PushNotificationCreatedEventHandler(ILogger<PushNotificationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PushNotificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "PushNotification created event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}, DeviceId: {DeviceId}, NotificationType: {NotificationType}, Title: {Title}",
            notification.PushNotificationId, notification.UserId, notification.DeviceId, notification.NotificationType, notification.Title);

        // TODO: İleride burada şunlar yapılabilir:
        // - Send push notification via FCM/APNS
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
