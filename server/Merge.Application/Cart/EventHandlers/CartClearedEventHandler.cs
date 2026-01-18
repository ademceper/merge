using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.EventHandlers;


public class CartClearedEventHandler(
    ILogger<CartClearedEventHandler> logger,
    ICacheService? cacheService = null) : INotificationHandler<CartClearedEvent>
{

    public async Task Handle(CartClearedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Cart cleared event received. CartId: {CartId}, UserId: {UserId}",
            notification.CartId, notification.UserId);

        try
        {
            if (cacheService != null)
            {
                await cacheService.RemoveAsync($"cart_{notification.CartId}", cancellationToken);
                await cacheService.RemoveAsync($"cart_user_{notification.UserId}", cancellationToken);
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
            logger.LogError(ex,
                "Error handling CartClearedEvent. CartId: {CartId}, UserId: {UserId}",
                notification.CartId, notification.UserId);
            throw;
        }
    }
}
