using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// GiftCard Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class GiftCardCreatedEventHandler(ILogger<GiftCardCreatedEventHandler> logger) : INotificationHandler<GiftCardCreatedEvent>
{
    public async Task Handle(GiftCardCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
