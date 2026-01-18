using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailTemplateActivatedEventHandler(ILogger<EmailTemplateActivatedEventHandler> logger) : INotificationHandler<EmailTemplateActivatedEvent>
{

    public async Task Handle(EmailTemplateActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailTemplate activated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
