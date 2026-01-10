using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// GiftCard Redeemed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class GiftCardRedeemedEventHandler : INotificationHandler<GiftCardRedeemedEvent>
{
    private readonly ILogger<GiftCardRedeemedEventHandler> _logger;

    public GiftCardRedeemedEventHandler(ILogger<GiftCardRedeemedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(GiftCardRedeemedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Gift card redeemed event received. GiftCardId: {GiftCardId}, Code: {Code}, AssignedToUserId: {AssignedToUserId}",
            notification.GiftCardId, notification.Code, notification.AssignedToUserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (gift card redeemed)

        await Task.CompletedTask;
    }
}
