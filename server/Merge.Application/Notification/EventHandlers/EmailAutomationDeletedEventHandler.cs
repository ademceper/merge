using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailAutomationDeletedEventHandler(ILogger<EmailAutomationDeletedEventHandler> logger) : INotificationHandler<EmailAutomationDeletedEvent>
{

    public async Task Handle(EmailAutomationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailAutomation deleted event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Stop automation scheduler
        // - Cleanup related data
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
