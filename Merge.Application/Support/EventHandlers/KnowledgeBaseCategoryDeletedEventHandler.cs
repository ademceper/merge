using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Knowledge Base Category Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class KnowledgeBaseCategoryDeletedEventHandler : INotificationHandler<KnowledgeBaseCategoryDeletedEvent>
{
    private readonly ILogger<KnowledgeBaseCategoryDeletedEventHandler> _logger;

    public KnowledgeBaseCategoryDeletedEventHandler(ILogger<KnowledgeBaseCategoryDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(KnowledgeBaseCategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Knowledge base category deleted event received. CategoryId: {CategoryId}, Name: {Name}, Slug: {Slug}, ParentCategoryId: {ParentCategoryId}",
            notification.CategoryId, notification.Name, notification.Slug, notification.ParentCategoryId);

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseCategoryDeletedAsync(notification, cancellationToken);

        // Invalidate cache if needed
        // await _cacheService.InvalidateAsync($"kb_category_{notification.CategoryId}", cancellationToken);

        await Task.CompletedTask;
    }
}
