using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class StaticTranslationUpdatedEventHandler(ILogger<StaticTranslationUpdatedEventHandler> logger) : INotificationHandler<StaticTranslationUpdatedEvent>
{
    public async Task Handle(StaticTranslationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Static translation updated event received. TranslationId: {TranslationId}, Key: {Key}, LanguageCode: {LanguageCode}",
            notification.TranslationId, notification.Key, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (static translations cache, UI translations cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling StaticTranslationUpdatedEvent. TranslationId: {TranslationId}, Key: {Key}",
                notification.TranslationId, notification.Key);
            throw;
        }
    }
}
