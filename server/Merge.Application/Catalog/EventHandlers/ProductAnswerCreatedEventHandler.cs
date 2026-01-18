using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductAnswerCreatedEventHandler(
    ILogger<ProductAnswerCreatedEventHandler> logger) : INotificationHandler<ProductAnswerCreatedEvent>
{

    public async Task Handle(ProductAnswerCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product answer created event received. AnswerId: {AnswerId}, QuestionId: {QuestionId}, UserId: {UserId}, IsSellerAnswer: {IsSellerAnswer}",
            notification.AnswerId, notification.QuestionId, notification.UserId, notification.IsSellerAnswer);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (question sahibine cevap bildirimi)
            // - Analytics tracking (answer creation metrics)
            // - Cache invalidation (product questions cache)
            // - Notification gönderimi (question sahibine)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductAnswerCreatedEvent. AnswerId: {AnswerId}, QuestionId: {QuestionId}",
                notification.AnswerId, notification.QuestionId);
            throw;
        }
    }
}
