using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Question Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductQuestionDeletedEventHandler : INotificationHandler<ProductQuestionDeletedEvent>
{
    private readonly ILogger<ProductQuestionDeletedEventHandler> _logger;

    public ProductQuestionDeletedEventHandler(ILogger<ProductQuestionDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductQuestionDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product question deleted event received. QuestionId: {QuestionId}, ProductId: {ProductId}, UserId: {UserId}",
            notification.QuestionId, notification.ProductId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product questions cache)
            // - Analytics tracking (question deletion metrics)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration (Q&A moderation service)
            // - Audit logging (question deletion audit trail)
            // - Cascade delete handling (answers, helpfulness votes)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductQuestionDeletedEvent. QuestionId: {QuestionId}, ProductId: {ProductId}, UserId: {UserId}",
                notification.QuestionId, notification.ProductId, notification.UserId);
            throw;
        }
    }
}
