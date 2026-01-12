using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Item Added Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartItemAddedEventHandler : INotificationHandler<CartItemAddedEvent>
{
    private readonly ILogger<CartItemAddedEventHandler> _logger;

    public CartItemAddedEventHandler(ILogger<CartItemAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CartItemAddedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Cart item added event received. CartId: {CartId}, ProductId: {ProductId}, Quantity: {Quantity}",
            notification.CartId, notification.ProductId, notification.Quantity);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (ürün sepete eklendi)
        // - Recommendation engine update (collaborative filtering)
        // - Cache invalidation
        // - Real-time inventory updates
        // - Marketing automation (cross-sell, upsell önerileri)

        await Task.CompletedTask;
    }
}
