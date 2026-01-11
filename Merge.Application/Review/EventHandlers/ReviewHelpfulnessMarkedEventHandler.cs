using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Review.EventHandlers;

/// <summary>
/// Review Helpfulness Marked Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReviewHelpfulnessMarkedEventHandler : INotificationHandler<ReviewHelpfulnessMarkedEvent>
{
    private readonly ILogger<ReviewHelpfulnessMarkedEventHandler> _logger;

    public ReviewHelpfulnessMarkedEventHandler(ILogger<ReviewHelpfulnessMarkedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReviewHelpfulnessMarkedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReviewHelpfulnessMarkedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
