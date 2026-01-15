using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyDeletedEventHandler(ILogger<CurrencyDeletedEventHandler> logger) : INotificationHandler<CurrencyDeletedEvent>
{
    public async Task Handle(CurrencyDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency deleted event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (currencies cache, exchange rates cache)
            // - Analytics tracking (currency deletion metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CurrencyDeletedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
