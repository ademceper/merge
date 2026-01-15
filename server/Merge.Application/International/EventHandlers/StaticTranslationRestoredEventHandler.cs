using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class StaticTranslationRestoredEventHandler(ILogger<StaticTranslationRestoredEventHandler> logger) : INotificationHandler<StaticTranslationRestoredEvent>
{
    public async Task Handle(StaticTranslationRestoredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Static translation restored event received. TranslationId: {TranslationId}, Key: {Key}, LanguageCode: {LanguageCode}",
            notification.Id, notification.Key, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (static translations cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling StaticTranslationRestoredEvent. TranslationId: {TranslationId}, Key: {Key}",
                notification.Id, notification.Key);
            throw;
        }
    }
}
