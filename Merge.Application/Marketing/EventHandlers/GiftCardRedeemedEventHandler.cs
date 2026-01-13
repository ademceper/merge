using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// GiftCard Redeemed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class GiftCardRedeemedEventHandler(ILogger<GiftCardRedeemedEventHandler> logger) : INotificationHandler<GiftCardRedeemedEvent>
{
    public async Task Handle(GiftCardRedeemedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Gift card redeemed event received. GiftCardId: {GiftCardId}, Code: {Code}, AssignedToUserId: {AssignedToUserId}",
            notification.GiftCardId, notification.Code, notification.AssignedToUserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (gift card redeemed)

        await Task.CompletedTask;
    }
}
