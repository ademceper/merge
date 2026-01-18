using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class CategoryUpdatedEventHandler(
    ILogger<CategoryUpdatedEventHandler> logger) : INotificationHandler<CategoryUpdatedEvent>
{

    public async Task Handle(CategoryUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling CategoryUpdatedEvent. CategoryId: {CategoryId}, Name: {Name}",
                notification.CategoryId, notification.Name);
            throw;
        }
    }
}
