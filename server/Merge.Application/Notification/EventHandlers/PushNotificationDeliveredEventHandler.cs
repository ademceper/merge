using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// PushNotification Delivered Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class PushNotificationDeliveredEventHandler(ILogger<PushNotificationDeliveredEventHandler> logger) : INotificationHandler<PushNotificationDeliveredEvent>
{

    public async Task Handle(PushNotificationDeliveredEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "PushNotification delivered event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}, DeliveredAt: {DeliveredAt}",
            notification.PushNotificationId, notification.UserId, notification.DeliveredAt);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (notification delivery rate)
        // - User engagement metrics

        await Task.CompletedTask;
    }
}
