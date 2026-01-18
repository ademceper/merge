using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationPreferenceUpdatedEventHandler(ILogger<NotificationPreferenceUpdatedEventHandler> logger) : INotificationHandler<NotificationPreferenceUpdatedEvent>
{

    public async Task Handle(NotificationPreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationPreference updated event received. PreferenceId: {PreferenceId}, UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}, IsEnabled: {IsEnabled}",
            notification.PreferenceId, notification.UserId, notification.NotificationType, notification.Channel, notification.IsEnabled);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - User preference sync to external systems

        await Task.CompletedTask;
    }
}
