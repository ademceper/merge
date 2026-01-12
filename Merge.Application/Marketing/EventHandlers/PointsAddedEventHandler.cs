using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Points Added Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class PointsAddedEventHandler : INotificationHandler<PointsAddedEvent>
{
    private readonly ILogger<PointsAddedEventHandler> _logger;

    public PointsAddedEventHandler(ILogger<PointsAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PointsAddedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Points added event received. AccountId: {AccountId}, UserId: {UserId}, Points: {Points}, NewBalance: {NewBalance}, Reason: {Reason}",
            notification.AccountId, notification.UserId, notification.Points, notification.NewBalance, notification.Reason);

        // TODO: İleride burada şunlar yapılabilir:
        // - Tier upgrade kontrolü
        // - Notification gönderimi (milestone achievements)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
