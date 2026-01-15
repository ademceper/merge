using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyActivatedEventHandler(ILogger<CurrencyActivatedEventHandler> logger) : INotificationHandler<CurrencyActivatedEvent>
{
    public async Task Handle(CurrencyActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency activated event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active currencies cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CurrencyActivatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
