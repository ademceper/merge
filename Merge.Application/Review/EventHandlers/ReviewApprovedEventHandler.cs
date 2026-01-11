using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Review.EventHandlers;

/// <summary>
/// Review Approved Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReviewApprovedEventHandler : INotificationHandler<ReviewApprovedEvent>
{
    private readonly ILogger<ReviewApprovedEventHandler> _logger;

    public ReviewApprovedEventHandler(ILogger<ReviewApprovedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReviewApprovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReviewApprovedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
