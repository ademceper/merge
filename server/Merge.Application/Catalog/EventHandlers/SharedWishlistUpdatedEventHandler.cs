using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Shared Wishlist Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SharedWishlistUpdatedEventHandler : INotificationHandler<SharedWishlistUpdatedEvent>
{
    private readonly ILogger<SharedWishlistUpdatedEventHandler> _logger;

    public SharedWishlistUpdatedEventHandler(ILogger<SharedWishlistUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SharedWishlistUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Shared wishlist updated event received. SharedWishlistId: {SharedWishlistId}, UserId: {UserId}, Name: {Name}",
            notification.SharedWishlistId, notification.UserId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (shared wishlists cache)
            // - Analytics tracking (wishlist update metrics)
            // - Real-time notification (SignalR, WebSocket - wishlist paylaşılan kullanıcılara)
            // - External system integration

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SharedWishlistUpdatedEvent. SharedWishlistId: {SharedWishlistId}, UserId: {UserId}",
                notification.SharedWishlistId, notification.UserId);
            throw;
        }
    }
}
