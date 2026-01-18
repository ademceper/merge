using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class KnowledgeBaseArticleDeletedEventHandler(ILogger<KnowledgeBaseArticleDeletedEventHandler> logger) : INotificationHandler<KnowledgeBaseArticleDeletedEvent>
{

    public async Task Handle(KnowledgeBaseArticleDeletedEvent notification, CancellationToken cancellationToken)
    {
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
