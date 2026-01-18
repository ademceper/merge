using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class ReferralCodeDeletedEventHandler(ILogger<ReferralCodeDeletedEventHandler> logger) : INotificationHandler<ReferralCodeDeletedEvent>
{
    public async Task Handle(ReferralCodeDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "ReferralCode deleted event received. ReferralCodeId: {ReferralCodeId}, UserId: {UserId}, Code: {Code}",
            notification.ReferralCodeId, notification.UserId, notification.Code);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}