using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class CategoryCreatedEventHandler(
    ILogger<CategoryCreatedEventHandler> logger) : INotificationHandler<CategoryCreatedEvent>
{

    public async Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Category created event received. CategoryId: {CategoryId}, Name: {Name}, Slug: {Slug}, ParentCategoryId: {ParentCategoryId}",
            notification.CategoryId, notification.Name, notification.Slug, notification.ParentCategoryId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (categories cache)
            // - Analytics tracking (category creation metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - External system integration (CMS, PIM)
            // - Notification gönderimi (admin'lere yeni kategori oluşturuldu bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CategoryCreatedEvent. CategoryId: {CategoryId}, Name: {Name}",
                notification.CategoryId, notification.Name);
            throw;
        }
    }
}
