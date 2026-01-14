using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Shared Wishlist Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SharedWishlistDeletedEventHandler : INotificationHandler<SharedWishlistDeletedEvent>
{
    private readonly ILogger<SharedWishlistDeletedEventHandler> _logger;

    public SharedWishlistDeletedEventHandler(ILogger<SharedWishlistDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SharedWishlistDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Shared wishlist deleted event received. SharedWishlistId: {SharedWishlistId}, UserId: {UserId}, Name: {Name}",
            notification.SharedWishlistId, notification.UserId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (shared wishlists cache)
            // - Analytics tracking (wishlist deletion metrics)
            // - Real-time notification (SignalR, WebSocket - wishlist paylaşılan kullanıcılara)
            // - External system integration
            // - Cascade delete handling (wishlist items)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SharedWishlistDeletedEvent. SharedWishlistId: {SharedWishlistId}, UserId: {UserId}",
                notification.SharedWishlistId, notification.UserId);
            throw;
        }
    }
}
