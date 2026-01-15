using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class LanguageRestoredEventHandler(ILogger<LanguageRestoredEventHandler> logger) : INotificationHandler<LanguageRestoredEvent>
{
    public async Task Handle(LanguageRestoredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Language restored event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (languages cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LanguageRestoredEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
