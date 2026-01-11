using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Review.EventHandlers;

/// <summary>
/// Review Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReviewUpdatedEventHandler : INotificationHandler<ReviewUpdatedEvent>
{
    private readonly ILogger<ReviewUpdatedEventHandler> _logger;

    public ReviewUpdatedEventHandler(ILogger<ReviewUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReviewUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReviewUpdatedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
