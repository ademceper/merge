using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Knowledge Base Category Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class KnowledgeBaseCategoryCreatedEventHandler : INotificationHandler<KnowledgeBaseCategoryCreatedEvent>
{
    private readonly ILogger<KnowledgeBaseCategoryCreatedEventHandler> _logger;

    public KnowledgeBaseCategoryCreatedEventHandler(ILogger<KnowledgeBaseCategoryCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(KnowledgeBaseCategoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Knowledge base category created event received. CategoryId: {CategoryId}, Name: {Name}, Slug: {Slug}, ParentCategoryId: {ParentCategoryId}",
            notification.CategoryId, notification.Name, notification.Slug, notification.ParentCategoryId);

        // Cache invalidation
        // await _cacheService.RemoveByTagAsync("knowledge-base-categories");

        // Analytics tracking
        // await _analyticsService.TrackKnowledgeBaseCategoryCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
