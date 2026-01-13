using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Notification.EventHandlers;

/// <summary>
/// PushNotification Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class PushNotificationDeletedEventHandler : INotificationHandler<PushNotificationDeletedEvent>
{
    private readonly ILogger<PushNotificationDeletedEventHandler> _logger;

    public PushNotificationDeletedEventHandler(ILogger<PushNotificationDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PushNotificationDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "PushNotification deleted event received. PushNotificationId: {PushNotificationId}, UserId: {UserId}",
            notification.PushNotificationId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (deletion rate)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
