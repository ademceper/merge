using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class CouponCreatedEventHandler(ILogger<CouponCreatedEventHandler> logger) : INotificationHandler<CouponCreatedEvent>
{
    public async Task Handle(CouponCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Coupon created event received. CouponId: {CouponId}, Code: {Code}, DiscountAmount: {DiscountAmount}",
            notification.CouponId, notification.Code, notification.DiscountAmount);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi
        // - Cache invalidation
        // - External system integration

        await Task.CompletedTask;
    }
}
