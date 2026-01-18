using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailAutomationDeactivatedEventHandler(ILogger<EmailAutomationDeactivatedEventHandler> logger) : INotificationHandler<EmailAutomationDeactivatedEvent>
{

    public async Task Handle(EmailAutomationDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailAutomation deactivated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Stop automation scheduler
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
