using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class LanguageUpdatedEventHandler(ILogger<LanguageUpdatedEventHandler> logger) : INotificationHandler<LanguageUpdatedEvent>
{
    public async Task Handle(LanguageUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Language updated event received. LanguageId: {LanguageId}, Code: {Code}, Name: {Name}",
            notification.LanguageId, notification.Code, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (languages cache, translations cache)
            // - Analytics tracking (language update metrics)
            // - Search index update (Elasticsearch, Algolia)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LanguageUpdatedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
