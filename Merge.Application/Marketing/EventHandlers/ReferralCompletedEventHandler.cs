using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Marketing.Commands.AddPoints;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Referral Completed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class ReferralCompletedEventHandler : INotificationHandler<ReferralCompletedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReferralCompletedEventHandler> _logger;

    public ReferralCompletedEventHandler(
        IMediator mediator,
        ILogger<ReferralCompletedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(ReferralCompletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Referral completed event received. ReferralId: {ReferralId}, ReferrerId: {ReferrerId}, ReferredUserId: {ReferredUserId}, PointsAwarded: {PointsAwarded}",
            notification.ReferralId, notification.ReferrerId, notification.ReferredUserId, notification.PointsAwarded);

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // Points award (LoyaltyAccount'a puan ekleme)
        var addPointsCommand = new AddPointsCommand(
            notification.ReferrerId,
            notification.PointsAwarded,
            "Referral",
            $"Referral reward for {notification.ReferredUserId}",
            null);
        await _mediator.Send(addPointsCommand, cancellationToken);

        // TODO: İleride burada şunlar yapılabilir:
        // - Notification gönderimi (referral completed)
        // - Analytics tracking
    }
}
