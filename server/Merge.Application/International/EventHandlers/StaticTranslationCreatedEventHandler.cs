using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class StaticTranslationCreatedEventHandler(ILogger<StaticTranslationCreatedEventHandler> logger) : INotificationHandler<StaticTranslationCreatedEvent>
{
    public async Task Handle(StaticTranslationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Static translation created event received. TranslationId: {TranslationId}, Key: {Key}, LanguageCode: {LanguageCode}, Category: {Category}",
            notification.TranslationId, notification.Key, notification.LanguageCode, notification.Category);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (static translations cache, UI translations cache)
            // - Analytics tracking (translation creation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling StaticTranslationCreatedEvent. TranslationId: {TranslationId}, Key: {Key}",
                notification.TranslationId, notification.Key);
            throw;
        }
    }
}
