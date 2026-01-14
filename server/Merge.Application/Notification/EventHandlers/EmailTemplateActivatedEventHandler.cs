using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// EmailTemplate Activated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailTemplateActivatedEventHandler : INotificationHandler<EmailTemplateActivatedEvent>
{
    private readonly ILogger<EmailTemplateActivatedEventHandler> _logger;

    public EmailTemplateActivatedEventHandler(ILogger<EmailTemplateActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailTemplateActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailTemplate activated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
