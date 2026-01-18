using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationTemplateDeletedEventHandler(ILogger<NotificationTemplateDeletedEventHandler> logger) : INotificationHandler<NotificationTemplateDeletedEvent>
{

    public async Task Handle(NotificationTemplateDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationTemplate deleted event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cleanup related data
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
