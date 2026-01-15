using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class LanguageActivatedEventHandler(ILogger<LanguageActivatedEventHandler> logger) : INotificationHandler<LanguageActivatedEvent>
{
    public async Task Handle(LanguageActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Language activated event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active languages cache)
            // - Analytics tracking (language activation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LanguageActivatedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
