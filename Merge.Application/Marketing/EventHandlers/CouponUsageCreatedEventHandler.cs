using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Coupon Usage Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class CouponUsageCreatedEventHandler : INotificationHandler<CouponUsageCreatedEvent>
{
    private readonly ILogger<CouponUsageCreatedEventHandler> _logger;

    public CouponUsageCreatedEventHandler(ILogger<CouponUsageCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CouponUsageCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Coupon usage created event received. CouponUsageId: {CouponUsageId}, CouponId: {CouponId}, UserId: {UserId}, OrderId: {OrderId}, DiscountAmount: {DiscountAmount}",
            notification.CouponUsageId,
            notification.CouponId,
            notification.UserId,
            notification.OrderId,
            notification.DiscountAmount);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (kupon kullanım istatistikleri)
        // - Notification gönderimi (kullanıcıya kupon kullanım onayı)
        // - Cache invalidation (kupon kullanım sayısı cache'i)
        // - External system integration (CRM, marketing automation)
        // - Loyalty points hesaplama (eğer kupon kullanımı puan kazandırıyorsa)

        await Task.CompletedTask;
    }
}
