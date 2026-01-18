using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class CouponUsedEventHandler(ILogger<CouponUsedEventHandler> logger) : INotificationHandler<CouponUsedEvent>
{
    public async Task Handle(CouponUsedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Coupon used event received. CouponId: {CouponId}, Code: {Code}, UsedCount: {UsedCount}, UsageLimit: {UsageLimit}",
            notification.CouponId, notification.Code, notification.UsedCount, notification.UsageLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Usage limit kontrolü
        // - Notification gönderimi

        await Task.CompletedTask;
    }
}
