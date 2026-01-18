using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationTemplateUpdatedEventHandler(ILogger<NotificationTemplateUpdatedEventHandler> logger) : INotificationHandler<NotificationTemplateUpdatedEvent>
{

    public async Task Handle(NotificationTemplateUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationTemplate updated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - External system integration

        await Task.CompletedTask;
    }
}
