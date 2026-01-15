using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyUpdatedEventHandler(ILogger<CurrencyUpdatedEventHandler> logger) : INotificationHandler<CurrencyUpdatedEvent>
{
    public async Task Handle(CurrencyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency updated event received. CurrencyId: {CurrencyId}, Code: {Code}, Name: {Name}",
            notification.CurrencyId, notification.Code, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (currencies cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CurrencyUpdatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
