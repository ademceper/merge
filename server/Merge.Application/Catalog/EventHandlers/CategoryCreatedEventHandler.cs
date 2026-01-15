using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Category Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CategoryCreatedEventHandler(
    ILogger<CategoryCreatedEventHandler> logger) : INotificationHandler<CategoryCreatedEvent>
{

    public async Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling CategoryCreatedEvent. CategoryId: {CategoryId}, Name: {Name}",
                notification.CategoryId, notification.Name);
            throw;
        }
    }
}
