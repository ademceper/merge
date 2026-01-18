using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class CategoryDeletedEventHandler(
    ILogger<CategoryDeletedEventHandler> logger) : INotificationHandler<CategoryDeletedEvent>
{

    public async Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling CategoryDeletedEvent. CategoryId: {CategoryId}, Name: {Name}",
                notification.CategoryId, notification.Name);
            throw;
        }
    }
}
