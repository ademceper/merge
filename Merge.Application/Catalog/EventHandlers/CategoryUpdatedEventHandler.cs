using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Category Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CategoryUpdatedEventHandler : INotificationHandler<CategoryUpdatedEvent>
{
    private readonly ILogger<CategoryUpdatedEventHandler> _logger;

    public CategoryUpdatedEventHandler(ILogger<CategoryUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CategoryUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Category updated event received. CategoryId: {CategoryId}, Name: {Name}, Slug: {Slug}",
            notification.CategoryId, notification.Name, notification.Slug);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (categories cache, category products cache)
            // - Analytics tracking (category update metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - External system integration (CMS, PIM)
            // - Notification gönderimi (admin'lere kategori güncellendi bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CategoryUpdatedEvent. CategoryId: {CategoryId}, Name: {Name}",
                notification.CategoryId, notification.Name);
            throw;
        }
    }
}
