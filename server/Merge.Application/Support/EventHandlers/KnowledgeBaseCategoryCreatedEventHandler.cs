using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class KnowledgeBaseCategoryCreatedEventHandler(ILogger<KnowledgeBaseCategoryCreatedEventHandler> logger) : INotificationHandler<KnowledgeBaseCategoryCreatedEvent>
{

    public async Task Handle(KnowledgeBaseCategoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Knowledge base category created event received. CategoryId: {CategoryId}, Name: {Name}, Slug: {Slug}, ParentCategoryId: {ParentCategoryId}",
            notification.CategoryId, notification.Name, notification.Slug, notification.ParentCategoryId);

        // Cache invalidation
        // await _cacheService.RemoveByTagAsync("knowledge-base-categories");

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseCategoryCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
