using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Cart.EventHandlers;

/// <summary>
/// Cart Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CartCreatedEventHandler : INotificationHandler<CartCreatedEvent>
{
    private readonly ILogger<CartCreatedEventHandler> _logger;

    public CartCreatedEventHandler(ILogger<CartCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CartCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Cart created event received. CartId: {CartId}, UserId: {UserId}",
            notification.CartId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (yeni sepet oluşturuldu)
        // - Cache invalidation
        // - External system integration
        // - Marketing automation (abandoned cart tracking başlat)

        await Task.CompletedTask;
    }
}
