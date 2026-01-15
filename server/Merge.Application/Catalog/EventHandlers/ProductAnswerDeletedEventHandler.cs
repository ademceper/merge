using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Answer Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductAnswerDeletedEventHandler(
    ILogger<ProductAnswerDeletedEventHandler> logger) : INotificationHandler<ProductAnswerDeletedEvent>
{

    public async Task Handle(ProductAnswerDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product answer deleted event received. AnswerId: {AnswerId}, QuestionId: {QuestionId}, UserId: {UserId}",
            notification.AnswerId, notification.QuestionId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product question/answer cache)
            // - Analytics tracking (answer deletion metrics)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration (Q&A moderation service)
            // - Audit logging (answer deletion audit trail)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ProductAnswerDeletedEvent. AnswerId: {AnswerId}, QuestionId: {QuestionId}, UserId: {UserId}",
                notification.AnswerId, notification.QuestionId, notification.UserId);
            throw;
        }
    }
}
