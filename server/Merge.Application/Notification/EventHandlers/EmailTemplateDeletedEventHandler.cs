using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailTemplateDeletedEventHandler(ILogger<EmailTemplateDeletedEventHandler> logger) : INotificationHandler<EmailTemplateDeletedEvent>
{

    public async Task Handle(EmailTemplateDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailTemplate deleted event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cleanup related data
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
