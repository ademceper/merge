using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Cleared Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartClearedEventHandler : INotificationHandler<CartClearedEvent>
{
    private readonly ILogger<CartClearedEventHandler> _logger;

    public CartClearedEventHandler(ILogger<CartClearedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CartClearedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Cart cleared event received. CartId: {CartId}, UserId: {UserId}",
            notification.CartId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (sepet temizlendi)
        // - Cache invalidation
        // - Real-time inventory updates
        // - Marketing automation (abandoned cart recovery başlat)

        await Task.CompletedTask;
    }
}
