using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// Notification Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationDeletedEventHandler : INotificationHandler<NotificationDeletedEvent>
{
    private readonly ILogger<NotificationDeletedEventHandler> _logger;

    public NotificationDeletedEventHandler(ILogger<NotificationDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(NotificationDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification deleted event received. NotificationId: {NotificationId}, UserId: {UserId}",
            notification.NotificationId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (notification deletion rate)
        // - User behavior analysis
        // - Cleanup of related data (if needed)
        // - Audit logging

        await Task.CompletedTask;
    }
}
