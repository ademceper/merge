using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// EmailTemplate Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailTemplateDeletedEventHandler : INotificationHandler<EmailTemplateDeletedEvent>
{
    private readonly ILogger<EmailTemplateDeletedEventHandler> _logger;

    public EmailTemplateDeletedEventHandler(ILogger<EmailTemplateDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailTemplateDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailTemplate deleted event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cleanup related data
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
