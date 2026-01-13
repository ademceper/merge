using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// EmailTemplate Deactivated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailTemplateDeactivatedEventHandler : INotificationHandler<EmailTemplateDeactivatedEvent>
{
    private readonly ILogger<EmailTemplateDeactivatedEventHandler> _logger;

    public EmailTemplateDeactivatedEventHandler(ILogger<EmailTemplateDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailTemplateDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailTemplate deactivated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
