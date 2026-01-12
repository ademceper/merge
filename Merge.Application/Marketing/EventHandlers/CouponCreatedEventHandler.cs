using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Coupon Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class CouponCreatedEventHandler : INotificationHandler<CouponCreatedEvent>
{
    private readonly ILogger<CouponCreatedEventHandler> _logger;

    public CouponCreatedEventHandler(ILogger<CouponCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CouponCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
