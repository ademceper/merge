using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyBaseCurrencyStatusRemovedEventHandler(ILogger<CurrencyBaseCurrencyStatusRemovedEventHandler> logger) : INotificationHandler<CurrencyBaseCurrencyStatusRemovedEvent>
{
    public async Task Handle(CurrencyBaseCurrencyStatusRemovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency base currency status removed event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (base currency cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CurrencyBaseCurrencyStatusRemovedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
