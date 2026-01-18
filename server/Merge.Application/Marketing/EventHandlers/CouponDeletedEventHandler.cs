using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class CouponDeletedEventHandler(ILogger<CouponDeletedEventHandler> logger) : INotificationHandler<CouponDeletedEvent>
{
    public async Task Handle(CouponDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Coupon deleted event received. CouponId: {CouponId}, Code: {Code}",
            notification.CouponId, notification.Code);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}
