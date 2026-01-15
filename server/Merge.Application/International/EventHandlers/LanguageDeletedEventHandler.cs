using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class LanguageDeletedEventHandler(ILogger<LanguageDeletedEventHandler> logger) : INotificationHandler<LanguageDeletedEvent>
{
    public async Task Handle(LanguageDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Language deleted event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (languages cache, translations cache)
            // - Analytics tracking (language deletion metrics)
            // - Search index update (Elasticsearch, Algolia)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LanguageDeletedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
