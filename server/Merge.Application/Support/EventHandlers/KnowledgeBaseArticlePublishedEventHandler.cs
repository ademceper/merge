using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Knowledge Base Article Published Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class KnowledgeBaseArticlePublishedEventHandler(ILogger<KnowledgeBaseArticlePublishedEventHandler> logger) : INotificationHandler<KnowledgeBaseArticlePublishedEvent>
{

    public async Task Handle(KnowledgeBaseArticlePublishedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Knowledge base article published event received. ArticleId: {ArticleId}, Title: {Title}, Slug: {Slug}, PublishedAt: {PublishedAt}",
            notification.ArticleId, notification.Title, notification.Slug, notification.PublishedAt);

        // Cache invalidation
        // await _cacheService.RemoveByTagAsync("knowledge-base");

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseArticlePublishedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
