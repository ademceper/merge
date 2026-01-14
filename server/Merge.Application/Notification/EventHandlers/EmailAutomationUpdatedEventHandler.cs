using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// EmailAutomation Updated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailAutomationUpdatedEventHandler : INotificationHandler<EmailAutomationUpdatedEvent>
{
    private readonly ILogger<EmailAutomationUpdatedEventHandler> _logger;

    public EmailAutomationUpdatedEventHandler(ILogger<EmailAutomationUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailAutomationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailAutomation updated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - External system integration

        await Task.CompletedTask;
    }
}
