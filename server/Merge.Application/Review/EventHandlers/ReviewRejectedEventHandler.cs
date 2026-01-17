using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Review.EventHandlers;

/// <summary>
/// Review Rejected Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReviewRejectedEventHandler(ILogger<ReviewRejectedEventHandler> logger) : INotificationHandler<ReviewRejectedEvent>
{

    public async Task Handle(ReviewRejectedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ReviewRejectedEvent. ReviewId: {ReviewId}, UserId: {UserId}",
                notification.ReviewId, notification.UserId);
            throw;
        }
    }
}
