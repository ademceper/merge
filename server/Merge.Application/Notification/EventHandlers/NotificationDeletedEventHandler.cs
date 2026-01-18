using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationDeletedEventHandler(ILogger<NotificationDeletedEventHandler> logger) : INotificationHandler<NotificationDeletedEvent>
{

    public async Task Handle(NotificationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
