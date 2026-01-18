using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationTemplateCreatedEventHandler(ILogger<NotificationTemplateCreatedEventHandler> logger) : INotificationHandler<NotificationTemplateCreatedEvent>
{

    public async Task Handle(NotificationTemplateCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationTemplate created event received. TemplateId: {TemplateId}, Name: {Name}, Type: {Type}",
            notification.TemplateId, notification.Name, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system integration
        // - Notification to admins

        await Task.CompletedTask;
    }
}
