using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class ProductTranslationUpdatedEventHandler(ILogger<ProductTranslationUpdatedEventHandler> logger) : INotificationHandler<ProductTranslationUpdatedEvent>
{
    public async Task Handle(ProductTranslationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product translation updated event received. TranslationId: {TranslationId}, ProductId: {ProductId}, LanguageCode: {LanguageCode}",
            notification.ProductTranslationId, notification.ProductId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product translations cache, product cache)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductTranslationUpdatedEvent. TranslationId: {TranslationId}, ProductId: {ProductId}",
                notification.ProductTranslationId, notification.ProductId);
            throw;
        }
    }
}
