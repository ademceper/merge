using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class KnowledgeBaseArticleCreatedEventHandler(ILogger<KnowledgeBaseArticleCreatedEventHandler> logger) : INotificationHandler<KnowledgeBaseArticleCreatedEvent>
{

    public async Task Handle(KnowledgeBaseArticleCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Knowledge base article created event received. ArticleId: {ArticleId}, Title: {Title}, Slug: {Slug}, AuthorId: {AuthorId}, CategoryId: {CategoryId}",
            notification.ArticleId, notification.Title, notification.Slug, notification.AuthorId, notification.CategoryId);

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseArticleCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
