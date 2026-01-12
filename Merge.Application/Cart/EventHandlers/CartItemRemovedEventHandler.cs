using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Item Removed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartItemRemovedEventHandler : INotificationHandler<CartItemRemovedEvent>
{
    private readonly ILogger<CartItemRemovedEventHandler> _logger;

    public CartItemRemovedEventHandler(ILogger<CartItemRemovedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CartItemRemovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Cart item removed event received. CartId: {CartId}, ProductId: {ProductId}",
            notification.CartId, notification.ProductId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (ürün sepetten çıkarıldı)
        // - Cache invalidation
        // - Real-time inventory updates
        // - Marketing automation (win-back campaigns)

        await Task.CompletedTask;
    }
}
