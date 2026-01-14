using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Coupon Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class CouponCreatedEventHandler(ILogger<CouponCreatedEventHandler> logger) : INotificationHandler<CouponCreatedEvent>
{
    public async Task Handle(CouponCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
