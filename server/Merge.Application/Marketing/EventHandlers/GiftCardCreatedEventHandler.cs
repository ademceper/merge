using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class GiftCardCreatedEventHandler(ILogger<GiftCardCreatedEventHandler> logger) : INotificationHandler<GiftCardCreatedEvent>
{
    public async Task Handle(GiftCardCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Gift card created event received. GiftCardId: {GiftCardId}, Code: {Code}, Amount: {Amount}, PurchasedByUserId: {PurchasedByUserId}, AssignedToUserId: {AssignedToUserId}",
            notification.GiftCardId, notification.Code, notification.Amount, notification.PurchasedByUserId, notification.AssignedToUserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Email gönderimi (gift card purchased/assigned)
        // - Analytics tracking
        // - Notification gönderimi

        await Task.CompletedTask;
    }
}
