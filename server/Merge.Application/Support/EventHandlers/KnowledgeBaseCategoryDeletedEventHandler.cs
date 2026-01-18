using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class KnowledgeBaseCategoryDeletedEventHandler(ILogger<KnowledgeBaseCategoryDeletedEventHandler> logger) : INotificationHandler<KnowledgeBaseCategoryDeletedEvent>
{

    public async Task Handle(KnowledgeBaseCategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Knowledge base category deleted event received. CategoryId: {CategoryId}, Name: {Name}, Slug: {Slug}, ParentCategoryId: {ParentCategoryId}",
            notification.CategoryId, notification.Name, notification.Slug, notification.ParentCategoryId);

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseCategoryDeletedAsync(notification, cancellationToken);

        // Invalidate cache if needed
        // await _cacheService.InvalidateAsync($"kb_category_{notification.CategoryId}", cancellationToken);

        await Task.CompletedTask;
    }
}
