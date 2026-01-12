using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

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
            "Email automation activated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Automation scheduler başlatma

        await Task.CompletedTask;
    }
}
