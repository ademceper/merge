using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyDeactivatedEventHandler(ILogger<CurrencyDeactivatedEventHandler> logger) : INotificationHandler<CurrencyDeactivatedEvent>
{
    public async Task Handle(CurrencyDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency deactivated event received. CurrencyId: {CurrencyId}, Code: {Code}",
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
                "Error handling CurrencyDeactivatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
