using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Coupon Used Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class CouponUsedEventHandler : INotificationHandler<CouponUsedEvent>
{
    private readonly ILogger<CouponUsedEventHandler> _logger;

    public CouponUsedEventHandler(ILogger<CouponUsedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CouponUsedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Coupon used event received. CouponId: {CouponId}, Code: {Code}, UsedCount: {UsedCount}, UsageLimit: {UsageLimit}",
            notification.CouponId, notification.Code, notification.UsedCount, notification.UsageLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Usage limit kontrolü
        // - Notification gönderimi

        await Task.CompletedTask;
    }
}
