using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductAnswerApprovedEventHandler(
    ILogger<ProductAnswerApprovedEventHandler> logger) : INotificationHandler<ProductAnswerApprovedEvent>
{

    public async Task Handle(ProductAnswerApprovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product answer approved event received. AnswerId: {AnswerId}, QuestionId: {QuestionId}",
            notification.AnswerId, notification.QuestionId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (user'a answer onaylandı bildirimi)
            // - Cache invalidation (product question/answer cache)
            // - Analytics tracking (answer approval metrics)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration (Q&A moderation service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductAnswerApprovedEvent. AnswerId: {AnswerId}, QuestionId: {QuestionId}",
                notification.AnswerId, notification.QuestionId);
            throw;
        }
    }
}
