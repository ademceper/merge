using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Knowledge Base Article Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class KnowledgeBaseArticleDeletedEventHandler(ILogger<KnowledgeBaseArticleDeletedEventHandler> logger) : INotificationHandler<KnowledgeBaseArticleDeletedEvent>
{

    public async Task Handle(KnowledgeBaseArticleDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Knowledge base article deleted event received. ArticleId: {ArticleId}, Title: {Title}, CategoryId: {CategoryId}",
            notification.ArticleId, notification.Title, notification.CategoryId);

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseArticleDeletedAsync(notification, cancellationToken);

        // Invalidate cache if needed
        // await _cacheService.InvalidateAsync($"kb_article_{notification.ArticleId}", cancellationToken);

        await Task.CompletedTask;
    }
}
