using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailTemplateCreatedEventHandler(ILogger<EmailTemplateCreatedEventHandler> logger) : INotificationHandler<EmailTemplateCreatedEvent>
{
    public async Task Handle(EmailTemplateCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email template created event received. TemplateId: {TemplateId}, Name: {Name}, Type: {Type}",
            notification.TemplateId, notification.Name, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
