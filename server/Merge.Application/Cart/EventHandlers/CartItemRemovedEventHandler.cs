using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Item Removed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartItemRemovedEventHandler(
    ILogger<CartItemRemovedEventHandler> logger,
    ICacheService? cacheService = null) : INotificationHandler<CartItemRemovedEvent>
{

    public async Task Handle(CartItemRemovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Cart item removed event received. CartId: {CartId}, ProductId: {ProductId}",
            notification.CartId, notification.ProductId);

        try
        {
            // ✅ BOLUM 10.2: Cache invalidation - Cart cache'i temizle
            if (cacheService != null)
            {
                await cacheService.RemoveAsync($"cart_{notification.CartId}", cancellationToken);
            }

            // Analytics tracking
            // TODO: Analytics service entegrasyonu eklendiğinde aktif edilecek
            // await _analyticsService.TrackCartItemRemovedAsync(notification, cancellationToken);

            // Real-time inventory updates
            // TODO: Inventory service entegrasyonu eklendiğinde aktif edilecek
            // await _inventoryService.ReleaseReservedStockAsync(notification.ProductId, cancellationToken);

            // Marketing automation (win-back campaigns)
            // TODO: Marketing automation service entegrasyonu eklendiğinde aktif edilecek
            // await _marketingService.TriggerWinBackCampaignAsync(notification.CartId, notification.ProductId, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling CartItemRemovedEvent. CartId: {CartId}, ProductId: {ProductId}",
                notification.CartId, notification.ProductId);
            throw;
        }
    }
}
