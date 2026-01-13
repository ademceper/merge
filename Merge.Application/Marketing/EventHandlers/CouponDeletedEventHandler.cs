using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Coupon Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class CouponDeletedEventHandler : INotificationHandler<CouponDeletedEvent>
{
    private readonly ILogger<CouponDeletedEventHandler> _logger;

    public CouponDeletedEventHandler(ILogger<CouponDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CouponDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Coupon deleted event received. CouponId: {CouponId}, Code: {Code}",
            notification.CouponId, notification.Code);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}
