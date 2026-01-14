using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Cleared Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartClearedEventHandler : INotificationHandler<CartClearedEvent>
{
    private readonly ILogger<CartClearedEventHandler> _logger;
    private readonly ICacheService? _cacheService;

    public CartClearedEventHandler(
        ILogger<CartClearedEventHandler> logger,
        ICacheService? cacheService = null)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(CartClearedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Cart cleared event received. CartId: {CartId}, UserId: {UserId}",
            notification.CartId, notification.UserId);

        try
        {
            // ✅ BOLUM 10.2: Cache invalidation - Cart cache'i temizle
            if (_cacheService != null)
            {
                await _cacheService.RemoveAsync($"cart_{notification.CartId}", cancellationToken);
                await _cacheService.RemoveAsync($"cart_user_{notification.UserId}", cancellationToken);
            }

            // Analytics tracking
            // TODO: Analytics service entegrasyonu eklendiğinde aktif edilecek
            // await _analyticsService.TrackCartClearedAsync(notification, cancellationToken);

            // Real-time inventory updates
            // TODO: Inventory service entegrasyonu eklendiğinde aktif edilecek
            // Tüm sepet öğeleri için reserved stock release edilmeli
            // await _inventoryService.ReleaseAllReservedStockForCartAsync(notification.CartId, cancellationToken);

            // Marketing automation (abandoned cart recovery başlat)
            // TODO: Marketing automation service entegrasyonu eklendiğinde aktif edilecek
            // await _marketingService.StartAbandonedCartRecoveryAsync(notification.CartId, notification.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CartClearedEvent. CartId: {CartId}, UserId: {UserId}",
                notification.CartId, notification.UserId);
            throw;
        }
    }
}
