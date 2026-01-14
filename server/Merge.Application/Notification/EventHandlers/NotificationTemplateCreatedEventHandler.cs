using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// NotificationTemplate Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationTemplateCreatedEventHandler : INotificationHandler<NotificationTemplateCreatedEvent>
{
    private readonly ILogger<NotificationTemplateCreatedEventHandler> _logger;

    public NotificationTemplateCreatedEventHandler(ILogger<NotificationTemplateCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(NotificationTemplateCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "NotificationTemplate created event received. TemplateId: {TemplateId}, Name: {Name}, Type: {Type}",
            notification.TemplateId, notification.Name, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system integration
        // - Notification to admins

        await Task.CompletedTask;
    }
}
