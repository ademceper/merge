using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// NotificationPreference Updated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationPreferenceUpdatedEventHandler(ILogger<NotificationPreferenceUpdatedEventHandler> logger) : INotificationHandler<NotificationPreferenceUpdatedEvent>
{

    public async Task Handle(NotificationPreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
