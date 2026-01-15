using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CategoryTranslationUpdatedEventHandler(ILogger<CategoryTranslationUpdatedEventHandler> logger) : INotificationHandler<CategoryTranslationUpdatedEvent>
{
    public async Task Handle(CategoryTranslationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Category translation updated event received. TranslationId: {TranslationId}, CategoryId: {CategoryId}, LanguageCode: {LanguageCode}",
            notification.CategoryTranslationId, notification.CategoryId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (category translations cache, category cache)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CategoryTranslationUpdatedEvent. TranslationId: {TranslationId}, CategoryId: {CategoryId}",
                notification.CategoryTranslationId, notification.CategoryId);
            throw;
        }
    }
}
