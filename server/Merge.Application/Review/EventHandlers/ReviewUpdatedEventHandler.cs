using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Review.EventHandlers;


public class ReviewUpdatedEventHandler(ILogger<ReviewUpdatedEventHandler> logger) : INotificationHandler<ReviewUpdatedEvent>
{

    public async Task Handle(ReviewUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Review updated event received. ReviewId: {ReviewId}, UserId: {UserId}, ProductId: {ProductId}, OldRating: {OldRating}, NewRating: {NewRating}",
            notification.ReviewId, notification.UserId, notification.ProductId, notification.OldRating, notification.NewRating);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product reviews cache, product rating cache)
            // - Analytics tracking (review update metrics)
            // - Review moderation queue'a tekrar ekleme (pending status'e döndü)
            // - Email bildirimi (seller'a review güncellendi bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReviewUpdatedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
