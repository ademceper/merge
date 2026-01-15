using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CategoryTranslationCreatedEventHandler(ILogger<CategoryTranslationCreatedEventHandler> logger) : INotificationHandler<CategoryTranslationCreatedEvent>
{
    public async Task Handle(CategoryTranslationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Category translation created event received. TranslationId: {TranslationId}, CategoryId: {CategoryId}, LanguageId: {LanguageId}, LanguageCode: {LanguageCode}, CategoryName: {CategoryName}",
            notification.CategoryTranslationId, notification.CategoryId, notification.LanguageId, notification.LanguageCode, notification.CategoryName);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (category translations cache, category cache)
            // - Analytics tracking (translation creation metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CategoryTranslationCreatedEvent. TranslationId: {TranslationId}, CategoryId: {CategoryId}",
                notification.CategoryTranslationId, notification.CategoryId);
            throw;
        }
    }
}
