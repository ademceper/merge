using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class NotificationTemplateDeactivatedEventHandler(ILogger<NotificationTemplateDeactivatedEventHandler> logger) : INotificationHandler<NotificationTemplateDeactivatedEvent>
{

    public async Task Handle(NotificationTemplateDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NotificationTemplate deactivated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
