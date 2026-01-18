using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailTemplateDeactivatedEventHandler(ILogger<EmailTemplateDeactivatedEventHandler> logger) : INotificationHandler<EmailTemplateDeactivatedEvent>
{

    public async Task Handle(EmailTemplateDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailTemplate deactivated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
