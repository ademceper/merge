using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;


public class EmailAutomationActivatedEventHandler(ILogger<EmailAutomationActivatedEventHandler> logger) : INotificationHandler<EmailAutomationActivatedEvent>
{

    public async Task Handle(EmailAutomationActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailAutomation activated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Start automation scheduler
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
