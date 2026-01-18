using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailTemplateUpdatedEventHandler(ILogger<EmailTemplateUpdatedEventHandler> logger) : INotificationHandler<EmailTemplateUpdatedEvent>
{

    public async Task Handle(EmailTemplateUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailTemplate updated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - External system integration

        await Task.CompletedTask;
    }
}
