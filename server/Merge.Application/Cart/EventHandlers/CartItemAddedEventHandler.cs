using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Item Added Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartItemAddedEventHandler(
    ILogger<CartItemAddedEventHandler> logger,
    ICacheService? cacheService = null) : INotificationHandler<CartItemAddedEvent>
{

    public async Task Handle(CartItemAddedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Cart item added event received. CartId: {CartId}, ProductId: {ProductId}, Quantity: {Quantity}",
            notification.CartId, notification.ProductId, notification.Quantity);

        try
        {
            // ✅ BOLUM 10.2: Cache invalidation - Cart cache'i temizle
            if (cacheService != null)
            {
                await cacheService.RemoveAsync($"cart_{notification.CartId}", cancellationToken);
                // User cart cache'i de temizle (cart user ID'den bulunabilir)
                // Not: UserId event'te yok, bu yüzden sadece cart ID cache'i temizleniyor
            }

            // Analytics tracking
            // TODO: Analytics service entegrasyonu eklendiğinde aktif edilecek
            // await _analyticsService.TrackCartItemAddedAsync(notification, cancellationToken);

            // Recommendation engine update (collaborative filtering)
            // TODO: Recommendation service entegrasyonu eklendiğinde aktif edilecek
            // await _recommendationService.UpdateCollaborativeFilteringAsync(notification.ProductId, cancellationToken);

            // Real-time inventory updates
            // TODO: Inventory service entegrasyonu eklendiğinde aktif edilecek
            // await _inventoryService.UpdateReservedStockAsync(notification.ProductId, notification.Quantity, cancellationToken);

            // Marketing automation (cross-sell, upsell önerileri)
            // TODO: Marketing automation service entegrasyonu eklendiğinde aktif edilecek
            // await _marketingService.TriggerCrossSellUpsellAsync(notification.CartId, notification.ProductId, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling CartItemAddedEvent. CartId: {CartId}, ProductId: {ProductId}",
                notification.CartId, notification.ProductId);
            throw;
        }
    }
}
