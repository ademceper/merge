using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartCreatedEventHandler : INotificationHandler<CartCreatedEvent>
{
    private readonly ILogger<CartCreatedEventHandler> _logger;
    private readonly ICacheService? _cacheService;

    public CartCreatedEventHandler(
        ILogger<CartCreatedEventHandler> logger,
        ICacheService? cacheService = null)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(CartCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Cart created event received. CartId: {CartId}, UserId: {UserId}",
            notification.CartId, notification.UserId);

        try
        {
            // ✅ BOLUM 10.2: Cache invalidation - User cart cache'i temizle
            if (_cacheService != null)
            {
                await _cacheService.RemoveAsync($"cart_user_{notification.UserId}", cancellationToken);
                await _cacheService.RemoveAsync($"cart_{notification.CartId}", cancellationToken);
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CartCreatedEvent. CartId: {CartId}, UserId: {UserId}",
                notification.CartId, notification.UserId);
            throw;
        }
    }
}
