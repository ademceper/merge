using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// NotificationTemplate Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class NotificationTemplateDeletedEventHandler : INotificationHandler<NotificationTemplateDeletedEvent>
{
    private readonly ILogger<NotificationTemplateDeletedEventHandler> _logger;

    public NotificationTemplateDeletedEventHandler(ILogger<NotificationTemplateDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(NotificationTemplateDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "NotificationTemplate deleted event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cleanup related data
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
