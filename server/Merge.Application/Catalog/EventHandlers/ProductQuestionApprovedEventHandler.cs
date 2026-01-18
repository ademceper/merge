using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductQuestionApprovedEventHandler(
    ILogger<ProductQuestionApprovedEventHandler> logger) : INotificationHandler<ProductQuestionApprovedEvent>
{

    public async Task Handle(ProductQuestionApprovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product question approved event received. QuestionId: {QuestionId}, ProductId: {ProductId}",
            notification.QuestionId, notification.ProductId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (user'a question onaylandı bildirimi)
            // - Cache invalidation (product questions cache)
            // - Analytics tracking (question approval metrics)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration (Q&A moderation service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductQuestionApprovedEvent. QuestionId: {QuestionId}, ProductId: {ProductId}",
                notification.QuestionId, notification.ProductId);
            throw;
        }
    }
}
