using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// PushNotification Failed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class PushNotificationFailedEventHandler(ILogger<PushNotificationFailedEventHandler> logger) : INotificationHandler<PushNotificationFailedEvent>
{

    public async Task Handle(PushNotificationFailedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
