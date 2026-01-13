using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// NotificationTemplate Deactivated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationTemplateDeactivatedEventHandler : INotificationHandler<NotificationTemplateDeactivatedEvent>
{
    private readonly ILogger<NotificationTemplateDeactivatedEventHandler> _logger;

    public NotificationTemplateDeactivatedEventHandler(ILogger<NotificationTemplateDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(NotificationTemplateDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "NotificationTemplate deactivated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
