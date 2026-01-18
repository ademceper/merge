using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.EventHandlers;


public class CartCreatedEventHandler(
    ILogger<CartCreatedEventHandler> logger,
    ICacheService? cacheService = null) : INotificationHandler<CartCreatedEvent>
{

    public async Task Handle(CartCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Cart created event received. CartId: {CartId}, UserId: {UserId}",
            notification.CartId, notification.UserId);

        try
        {
            if (cacheService is not null)
            {
                await cacheService.RemoveAsync($"cart_user_{notification.UserId}", cancellationToken);
                await cacheService.RemoveAsync($"cart_{notification.CartId}", cancellationToken);
            }

            // Analytics tracking
            // TODO: Analytics service entegrasyonu eklendiğinde aktif edilecek
            // await _analyticsService.TrackCartCreatedAsync(notification, cancellationToken);

            // Marketing automation (abandoned cart tracking başlat)
            // TODO: Marketing automation service entegrasyonu eklendiğinde aktif edilecek
            // await _marketingService.StartAbandonedCartTrackingAsync(notification.CartId, notification.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CartCreatedEvent. CartId: {CartId}, UserId: {UserId}",
                notification.CartId, notification.UserId);
            throw;
        }
    }
}
