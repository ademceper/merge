using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// NotificationPreference Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationPreferenceDeletedEventHandler : INotificationHandler<NotificationPreferenceDeletedEvent>
{
    private readonly ILogger<NotificationPreferenceDeletedEventHandler> _logger;

    public NotificationPreferenceDeletedEventHandler(ILogger<NotificationPreferenceDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(NotificationPreferenceDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "NotificationPreference deleted event received. PreferenceId: {PreferenceId}, UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            notification.PreferenceId, notification.UserId, notification.NotificationType, notification.Channel);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - User preference sync to external systems

        await Task.CompletedTask;
    }
}
