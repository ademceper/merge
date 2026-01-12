using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Category Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CategoryDeletedEventHandler : INotificationHandler<CategoryDeletedEvent>
{
    private readonly ILogger<CategoryDeletedEventHandler> _logger;

    public CategoryDeletedEventHandler(ILogger<CategoryDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Category deleted event received. CategoryId: {CategoryId}, Name: {Name}",
            notification.CategoryId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (categories cache, category products cache)
            // - Analytics tracking (category deletion metrics)
            // - Search index update (Elasticsearch, Algolia - remove category)
            // - External system integration (CMS, PIM)
            // - Notification gönderimi (admin'lere kategori silindi bildirimi)
            // - Cascade delete handling (subcategories, products)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CategoryDeletedEvent. CategoryId: {CategoryId}, Name: {Name}",
                notification.CategoryId, notification.Name);
            throw;
        }
    }
}
