using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Review.EventHandlers;

/// <summary>
/// Review Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReviewCreatedEventHandler : INotificationHandler<ReviewCreatedEvent>
{
    private readonly ILogger<ReviewCreatedEventHandler> _logger;

    public ReviewCreatedEventHandler(ILogger<ReviewCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReviewCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Review created event received. ReviewId: {ReviewId}, UserId: {UserId}, ProductId: {ProductId}, Rating: {Rating}, IsVerifiedPurchase: {IsVerifiedPurchase}",
            notification.ReviewId, notification.UserId, notification.ProductId, notification.Rating, notification.IsVerifiedPurchase);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (seller'a yeni review bildirimi)
            // - Analytics tracking (review creation metrics)
            // - Cache invalidation (product reviews cache)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration (review moderation service)
            // - Fraud detection (ML service)
            // - Review moderation queue'a ekleme

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReviewCreatedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
