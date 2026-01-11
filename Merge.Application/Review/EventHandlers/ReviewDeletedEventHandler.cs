using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Review.EventHandlers;

/// <summary>
/// Review Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReviewDeletedEventHandler : INotificationHandler<ReviewDeletedEvent>
{
    private readonly ILogger<ReviewDeletedEventHandler> _logger;

    public ReviewDeletedEventHandler(ILogger<ReviewDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReviewDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReviewDeletedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
