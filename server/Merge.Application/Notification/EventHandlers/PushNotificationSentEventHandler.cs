using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// PushNotification Sent Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class PushNotificationSentEventHandler(ILogger<PushNotificationSentEventHandler> logger) : INotificationHandler<PushNotificationSentEvent>
{

    public async Task Handle(PushNotificationSentEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "PushNotification sent event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}, SentAt: {SentAt}",
            notification.PushNotificationId, notification.UserId, notification.SentAt);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (notification sent rate)
        // - Metrics collection

        await Task.CompletedTask;
    }
}
