using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationTemplateActivatedEventHandler(ILogger<NotificationTemplateActivatedEventHandler> logger) : INotificationHandler<NotificationTemplateActivatedEvent>
{

    public async Task Handle(NotificationTemplateActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationTemplate activated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
