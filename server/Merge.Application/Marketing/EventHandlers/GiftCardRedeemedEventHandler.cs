using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class GiftCardRedeemedEventHandler(ILogger<GiftCardRedeemedEventHandler> logger) : INotificationHandler<GiftCardRedeemedEvent>
{
    public async Task Handle(GiftCardRedeemedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Gift card redeemed event received. GiftCardId: {GiftCardId}, Code: {Code}, AssignedToUserId: {AssignedToUserId}",
            notification.GiftCardId, notification.Code, notification.AssignedToUserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (gift card redeemed)

        await Task.CompletedTask;
    }
}
