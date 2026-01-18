using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class PointsAddedEventHandler(ILogger<PointsAddedEventHandler> logger) : INotificationHandler<PointsAddedEvent>
{
    public async Task Handle(PointsAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Points added event received. AccountId: {AccountId}, UserId: {UserId}, Points: {Points}, NewBalance: {NewBalance}, Reason: {Reason}",
            notification.AccountId, notification.UserId, notification.Points, notification.NewBalance, notification.Reason);

        // TODO: İleride burada şunlar yapılabilir:
        // - Tier upgrade kontrolü
        // - Notification gönderimi (milestone achievements)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
