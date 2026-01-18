using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class ReferralCodeCreatedEventHandler(ILogger<ReferralCodeCreatedEventHandler> logger) : INotificationHandler<ReferralCodeCreatedEvent>
{
    public async Task Handle(ReferralCodeCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Referral code created event received. ReferralCodeId: {ReferralCodeId}, UserId: {UserId}, Code: {Code}",
            notification.ReferralCodeId, notification.UserId, notification.Code);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (referral code ready)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
