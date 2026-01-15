using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CategoryTranslationDeletedEventHandler(ILogger<CategoryTranslationDeletedEventHandler> logger) : INotificationHandler<CategoryTranslationDeletedEvent>
{
    public async Task Handle(CategoryTranslationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Category translation deleted event received. TranslationId: {TranslationId}, CategoryId: {CategoryId}, LanguageCode: {LanguageCode}",
            notification.CategoryTranslationId, notification.CategoryId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (category translations cache, category cache)
            // - Search index update (Elasticsearch, Algolia)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CategoryTranslationDeletedEvent. TranslationId: {TranslationId}, CategoryId: {CategoryId}",
                notification.CategoryTranslationId, notification.CategoryId);
            throw;
        }
    }
}
