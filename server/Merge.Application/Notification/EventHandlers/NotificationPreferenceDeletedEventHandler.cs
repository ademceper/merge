using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationPreferenceDeletedEventHandler(ILogger<NotificationPreferenceDeletedEventHandler> logger) : INotificationHandler<NotificationPreferenceDeletedEvent>
{

    public async Task Handle(NotificationPreferenceDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationPreference deleted event received. PreferenceId: {PreferenceId}, UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            notification.PreferenceId, notification.UserId, notification.NotificationType, notification.Channel);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - User preference sync to external systems

        await Task.CompletedTask;
    }
}
