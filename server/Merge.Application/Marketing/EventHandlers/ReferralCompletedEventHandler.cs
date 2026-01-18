using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Marketing.Commands.AddPoints;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class ReferralCompletedEventHandler(
    IMediator mediator,
    ILogger<ReferralCompletedEventHandler> logger) : INotificationHandler<ReferralCompletedEvent>
{
    public async Task Handle(ReferralCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Referral completed event received. ReferralId: {ReferralId}, ReferrerId: {ReferrerId}, ReferredUserId: {ReferredUserId}, PointsAwarded: {PointsAwarded}",
            notification.ReferralId, notification.ReferrerId, notification.ReferredUserId, notification.PointsAwarded);

        // Points award (LoyaltyAccount'a puan ekleme)
        var addPointsCommand = new AddPointsCommand(
            notification.ReferrerId,
            notification.PointsAwarded,
            "Referral",
            $"Referral reward for {notification.ReferredUserId}",
            null);
        await mediator.Send(addPointsCommand, cancellationToken);

        // TODO: İleride burada şunlar yapılabilir:
        // - Notification gönderimi (referral completed)
        // - Analytics tracking
    }
}
