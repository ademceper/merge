using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class ProductTranslationCreatedEventHandler(ILogger<ProductTranslationCreatedEventHandler> logger) : INotificationHandler<ProductTranslationCreatedEvent>
{
    public async Task Handle(ProductTranslationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product translation created event received. TranslationId: {TranslationId}, ProductId: {ProductId}, LanguageId: {LanguageId}, LanguageCode: {LanguageCode}, ProductName: {ProductName}",
            notification.ProductTranslationId, notification.ProductId, notification.LanguageId, notification.LanguageCode, notification.ProductName);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product translations cache, product cache)
            // - Analytics tracking (translation creation metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductTranslationCreatedEvent. TranslationId: {TranslationId}, ProductId: {ProductId}",
                notification.ProductTranslationId, notification.ProductId);
            throw;
        }
    }
}
