using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class PushNotificationSentEventHandler(ILogger<PushNotificationSentEventHandler> logger) : INotificationHandler<PushNotificationSentEvent>
{

    public async Task Handle(PushNotificationSentEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "PushNotification sent event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}, SentAt: {SentAt}",
            notification.PushNotificationId, notification.UserId, notification.SentAt);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (notification sent rate)
        // - Metrics collection

        await Task.CompletedTask;
    }
}
