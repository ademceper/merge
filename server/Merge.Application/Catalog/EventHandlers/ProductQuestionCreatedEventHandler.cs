using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductQuestionCreatedEventHandler(
    ILogger<ProductQuestionCreatedEventHandler> logger) : INotificationHandler<ProductQuestionCreatedEvent>
{

    public async Task Handle(ProductQuestionCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product question created event received. QuestionId: {QuestionId}, ProductId: {ProductId}, UserId: {UserId}, Question: {Question}",
            notification.QuestionId, notification.ProductId, notification.UserId, notification.Question);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (seller'a yeni soru bildirimi)
            // - Analytics tracking (question creation metrics)
            // - Cache invalidation (product questions cache)
            // - Notification gönderimi (seller'a)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductQuestionCreatedEvent. QuestionId: {QuestionId}, ProductId: {ProductId}",
                notification.QuestionId, notification.ProductId);
            throw;
        }
    }
}
