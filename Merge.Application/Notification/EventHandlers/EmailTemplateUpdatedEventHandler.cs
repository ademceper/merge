using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// EmailTemplate Updated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailTemplateUpdatedEventHandler : INotificationHandler<EmailTemplateUpdatedEvent>
{
    private readonly ILogger<EmailTemplateUpdatedEventHandler> _logger;

    public EmailTemplateUpdatedEventHandler(ILogger<EmailTemplateUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailTemplateUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailTemplate updated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - External system integration

        await Task.CompletedTask;
    }
}
