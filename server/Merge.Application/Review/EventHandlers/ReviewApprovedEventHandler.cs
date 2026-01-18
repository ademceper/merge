using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Review.EventHandlers;


public class ReviewApprovedEventHandler(ILogger<ReviewApprovedEventHandler> logger) : INotificationHandler<ReviewApprovedEvent>
{

    public async Task Handle(ReviewApprovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Review approved event received. ReviewId: {ReviewId}, UserId: {UserId}, ProductId: {ProductId}, Rating: {Rating}, ApprovedByUserId: {ApprovedByUserId}",
            notification.ReviewId, notification.UserId, notification.ProductId, notification.Rating, notification.ApprovedByUserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (user'a review onaylandı bildirimi)
            // - Cache invalidation (product reviews cache, product rating cache)
            // - Analytics tracking (review approval metrics)
            // - Product rating güncelleme (zaten UpdateProductRatingAsync'de yapılıyor ama event handler'da da yapılabilir)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration (review aggregation service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReviewApprovedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
