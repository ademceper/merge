using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class SharedWishlistCreatedEventHandler(
    ILogger<SharedWishlistCreatedEventHandler> logger) : INotificationHandler<SharedWishlistCreatedEvent>
{

    public async Task Handle(SharedWishlistCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling SharedWishlistCreatedEvent. SharedWishlistId: {SharedWishlistId}, UserId: {UserId}",
                notification.SharedWishlistId, notification.UserId);
            throw;
        }
    }
}
