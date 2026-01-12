using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailTemplate Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailTemplateCreatedEventHandler : INotificationHandler<EmailTemplateCreatedEvent>
{
    private readonly ILogger<EmailTemplateCreatedEventHandler> _logger;

    public EmailTemplateCreatedEventHandler(ILogger<EmailTemplateCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailTemplateCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email template created event received. TemplateId: {TemplateId}, Name: {Name}, Type: {Type}",
            notification.TemplateId, notification.Name, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
