using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class StaticTranslationDeletedEventHandler(ILogger<StaticTranslationDeletedEventHandler> logger) : INotificationHandler<StaticTranslationDeletedEvent>
{
    public async Task Handle(StaticTranslationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Static translation deleted event received. TranslationId: {TranslationId}, Key: {Key}, LanguageCode: {LanguageCode}",
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
                "Error handling StaticTranslationDeletedEvent. TranslationId: {TranslationId}, Key: {Key}",
                notification.TranslationId, notification.Key);
            throw;
        }
    }
}
