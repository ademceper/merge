using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Review.EventHandlers;


public class ReviewRejectedEventHandler(ILogger<ReviewRejectedEventHandler> logger) : INotificationHandler<ReviewRejectedEvent>
{

    public async Task Handle(ReviewRejectedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Review rejected event received. ReviewId: {ReviewId}, UserId: {UserId}, ProductId: {ProductId}, RejectedByUserId: {RejectedByUserId}, Reason: {Reason}",
            notification.ReviewId, notification.UserId, notification.ProductId, notification.RejectedByUserId, notification.Reason);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (user'a review reddedildi bildirimi + sebep)
            // - Cache invalidation (product reviews cache)
            // - Analytics tracking (review rejection metrics)
            // - Fraud detection feedback (ML service'e feedback)
            // - Review moderation log'a kayıt

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReviewRejectedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
