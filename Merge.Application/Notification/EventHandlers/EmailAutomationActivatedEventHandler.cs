using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// EmailAutomation Activated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailAutomationActivatedEventHandler : INotificationHandler<EmailAutomationActivatedEvent>
{
    private readonly ILogger<EmailAutomationActivatedEventHandler> _logger;

    public EmailAutomationActivatedEventHandler(ILogger<EmailAutomationActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailAutomationActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailAutomation activated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Start automation scheduler
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
