using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailAutomationActivatedEventHandler(ILogger<EmailAutomationActivatedEventHandler> logger) : INotificationHandler<EmailAutomationActivatedEvent>
{
    public async Task Handle(EmailAutomationActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email automation activated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Automation scheduler başlatma

        await Task.CompletedTask;
    }
}
