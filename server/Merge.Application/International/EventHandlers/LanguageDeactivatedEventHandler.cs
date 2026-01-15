using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class LanguageDeactivatedEventHandler(ILogger<LanguageDeactivatedEventHandler> logger) : INotificationHandler<LanguageDeactivatedEvent>
{
    public async Task Handle(LanguageDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Language deactivated event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active languages cache)
            // - Analytics tracking (language deactivation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LanguageDeactivatedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
