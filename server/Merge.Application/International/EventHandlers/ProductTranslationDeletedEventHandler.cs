using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class ProductTranslationDeletedEventHandler(ILogger<ProductTranslationDeletedEventHandler> logger) : INotificationHandler<ProductTranslationDeletedEvent>
{
    public async Task Handle(ProductTranslationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product translation deleted event received. TranslationId: {TranslationId}, ProductId: {ProductId}, LanguageCode: {LanguageCode}",
            notification.ProductTranslationId, notification.ProductId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product translations cache, product cache)
            // - Search index update (Elasticsearch, Algolia)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductTranslationDeletedEvent. TranslationId: {TranslationId}, ProductId: {ProductId}",
                notification.ProductTranslationId, notification.ProductId);
            throw;
        }
    }
}
