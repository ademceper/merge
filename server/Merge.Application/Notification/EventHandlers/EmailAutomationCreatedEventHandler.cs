using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailAutomationCreatedEventHandler(ILogger<EmailAutomationCreatedEventHandler> logger) : INotificationHandler<EmailAutomationCreatedEvent>
{

    public async Task Handle(EmailAutomationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailAutomation created event received. AutomationId: {AutomationId}, Name: {Name}, Type: {Type}",
            notification.AutomationId, notification.Name, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system integration
        // - Notification to admins

        await Task.CompletedTask;
    }
}
