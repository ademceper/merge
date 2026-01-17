using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// NotificationPreference Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationPreferenceCreatedEventHandler(ILogger<NotificationPreferenceCreatedEventHandler> logger) : INotificationHandler<NotificationPreferenceCreatedEvent>
{

    public async Task Handle(NotificationPreferenceCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "NotificationPreference created event received. PreferenceId: {PreferenceId}, UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}, IsEnabled: {IsEnabled}",
            notification.PreferenceId, notification.UserId, notification.NotificationType, notification.Channel, notification.IsEnabled);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - User preference sync to external systems

        await Task.CompletedTask;
    }
}
