using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyCreatedEventHandler(ILogger<CurrencyCreatedEventHandler> logger) : INotificationHandler<CurrencyCreatedEvent>
{
    public async Task Handle(CurrencyCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Currency created event received. CurrencyId: {CurrencyId}, Code: {Code}, Name: {Name}",
            notification.CurrencyId, notification.Code, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (currencies cache, exchange rates cache)
            // - Analytics tracking (currency creation metrics)
            // - External system integration (Payment gateways, Financial systems)
            // - Notification gönderimi (admin'lere yeni para birimi oluşturuldu bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling CurrencyCreatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
