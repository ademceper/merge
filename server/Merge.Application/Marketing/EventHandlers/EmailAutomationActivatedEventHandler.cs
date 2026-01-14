using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailAutomation Activated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailAutomationActivatedEventHandler(ILogger<EmailAutomationActivatedEventHandler> logger) : INotificationHandler<EmailAutomationActivatedEvent>
{
    public async Task Handle(EmailAutomationActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email automation activated event received. AutomationId: {AutomationId}, Name: {Name}",
            notification.AutomationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Automation scheduler başlatma

        await Task.CompletedTask;
    }
}
