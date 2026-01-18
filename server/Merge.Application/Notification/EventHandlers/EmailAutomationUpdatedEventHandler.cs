using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailAutomationUpdatedEventHandler(ILogger<EmailAutomationUpdatedEventHandler> logger) : INotificationHandler<EmailAutomationUpdatedEvent>
{

    public async Task Handle(EmailAutomationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailAutomation updated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - External system integration

        await Task.CompletedTask;
    }
}
