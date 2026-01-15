using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Answer Approved Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductAnswerApprovedEventHandler(
    ILogger<ProductAnswerApprovedEventHandler> logger) : INotificationHandler<ProductAnswerApprovedEvent>
{

    public async Task Handle(ProductAnswerApprovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ProductAnswerApprovedEvent. AnswerId: {AnswerId}, QuestionId: {QuestionId}",
                notification.AnswerId, notification.QuestionId);
            throw;
        }
    }
}
