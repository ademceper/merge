using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyRestoredEventHandler(ILogger<CurrencyRestoredEventHandler> logger) : INotificationHandler<CurrencyRestoredEvent>
{
    public async Task Handle(CurrencyRestoredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency restored event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (currencies cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CurrencyRestoredEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
