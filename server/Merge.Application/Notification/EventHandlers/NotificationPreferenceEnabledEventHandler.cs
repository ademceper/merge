using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationPreferenceEnabledEventHandler(ILogger<NotificationPreferenceEnabledEventHandler> logger) : INotificationHandler<NotificationPreferenceEnabledEvent>
{

    public async Task Handle(NotificationPreferenceEnabledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationPreference enabled event received. PreferenceId: {PreferenceId}, UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            notification.PreferenceId, notification.UserId, notification.NotificationType, notification.Channel);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - User preference sync to external systems

        await Task.CompletedTask;
    }
}
