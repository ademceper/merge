using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// NotificationTemplate Activated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationTemplateActivatedEventHandler : INotificationHandler<NotificationTemplateActivatedEvent>
{
    private readonly ILogger<NotificationTemplateActivatedEventHandler> _logger;

    public NotificationTemplateActivatedEventHandler(ILogger<NotificationTemplateActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(NotificationTemplateActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "NotificationTemplate activated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
