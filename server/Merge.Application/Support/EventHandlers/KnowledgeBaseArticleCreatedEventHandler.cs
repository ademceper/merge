using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Knowledge Base Article Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class KnowledgeBaseArticleCreatedEventHandler(ILogger<KnowledgeBaseArticleCreatedEventHandler> logger) : INotificationHandler<KnowledgeBaseArticleCreatedEvent>
{

    public async Task Handle(KnowledgeBaseArticleCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Knowledge base article created event received. ArticleId: {ArticleId}, Title: {Title}, Slug: {Slug}, AuthorId: {AuthorId}, CategoryId: {CategoryId}",
            notification.ArticleId, notification.Title, notification.Slug, notification.AuthorId, notification.CategoryId);

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseArticleCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
