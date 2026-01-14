using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Shared Wishlist Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SharedWishlistCreatedEventHandler : INotificationHandler<SharedWishlistCreatedEvent>
{
    private readonly ILogger<SharedWishlistCreatedEventHandler> _logger;

    public SharedWishlistCreatedEventHandler(ILogger<SharedWishlistCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SharedWishlistCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Shared wishlist created event received. SharedWishlistId: {SharedWishlistId}, UserId: {UserId}, Name: {Name}, IsPublic: {IsPublic}",
            notification.SharedWishlistId, notification.UserId, notification.Name, notification.IsPublic);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (wishlist creation metrics)
            // - Cache invalidation (user wishlists cache)
            // - Notification gönderimi (wishlist paylaşıldığında)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SharedWishlistCreatedEvent. SharedWishlistId: {SharedWishlistId}, UserId: {UserId}",
                notification.SharedWishlistId, notification.UserId);
            throw;
        }
    }
}
