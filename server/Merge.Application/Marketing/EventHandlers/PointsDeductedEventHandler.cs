using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class PointsDeductedEventHandler(ILogger<PointsDeductedEventHandler> logger) : INotificationHandler<PointsDeductedEvent>
{
    public async Task Handle(PointsDeductedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Points deducted event received. AccountId: {AccountId}, UserId: {UserId}, Points: {Points}, NewBalance: {NewBalance}, Reason: {Reason}",
            notification.AccountId, notification.UserId, notification.Points, notification.NewBalance, notification.Reason);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (points redeemed)

        await Task.CompletedTask;
    }
}
