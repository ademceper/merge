using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// EmailAutomation Deactivated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailAutomationDeactivatedEventHandler(ILogger<EmailAutomationDeactivatedEventHandler> logger) : INotificationHandler<EmailAutomationDeactivatedEvent>
{

    public async Task Handle(EmailAutomationDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
