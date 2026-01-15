using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencySetAsBaseEventHandler(ILogger<CurrencySetAsBaseEventHandler> logger) : INotificationHandler<CurrencySetAsBaseEvent>
{
    public async Task Handle(CurrencySetAsBaseEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency set as base event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (base currency cache, all exchange rates cache)
            // - Analytics tracking (base currency change metrics)
            // - Notification gönderimi (admin'lere base currency değişikliği bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CurrencySetAsBaseEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
