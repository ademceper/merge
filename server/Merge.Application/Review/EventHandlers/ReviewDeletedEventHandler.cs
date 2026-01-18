using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Review.EventHandlers;


public class ReviewDeletedEventHandler(ILogger<ReviewDeletedEventHandler> logger) : INotificationHandler<ReviewDeletedEvent>
{

    public async Task Handle(ReviewDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Review deleted event received. ReviewId: {ReviewId}, UserId: {UserId}, ProductId: {ProductId}",
            notification.ReviewId, notification.UserId, notification.ProductId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product reviews cache, product rating cache)
            // - Analytics tracking (review deletion metrics)
            // - Product rating güncelleme (zaten UpdateProductRatingAsync'de yapılıyor ama event handler'da da yapılabilir)
            // - Review media silme (ReviewMedia entity'lerini de sil)
            // - Review helpfulness votes silme (ReviewHelpfulness entity'lerini de sil)
            // - External system integration (review aggregation service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReviewDeletedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
