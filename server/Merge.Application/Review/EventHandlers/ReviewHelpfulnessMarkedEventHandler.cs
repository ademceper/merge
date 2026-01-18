using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Review.EventHandlers;


public class ReviewHelpfulnessMarkedEventHandler(ILogger<ReviewHelpfulnessMarkedEventHandler> logger) : INotificationHandler<ReviewHelpfulnessMarkedEvent>
{

    public async Task Handle(ReviewHelpfulnessMarkedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Review helpfulness marked event received. ReviewId: {ReviewId}, UserId: {UserId}, IsHelpful: {IsHelpful}",
            notification.ReviewId, notification.UserId, notification.IsHelpful);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (helpfulness vote metrics)
            // - Cache invalidation (review helpfulness stats cache)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration (review aggregation service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReviewHelpfulnessMarkedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
