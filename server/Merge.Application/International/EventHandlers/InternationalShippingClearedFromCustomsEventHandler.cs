using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class InternationalShippingClearedFromCustomsEventHandler(ILogger<InternationalShippingClearedFromCustomsEventHandler> logger) : INotificationHandler<InternationalShippingClearedFromCustomsEvent>
{
    public async Task Handle(InternationalShippingClearedFromCustomsEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "International shipping cleared from customs event received. ShippingId: {ShippingId}, OrderId: {OrderId}",
            notification.InternationalShippingId, notification.OrderId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo gümrükten çıktı bildirimi)
            // - Analytics tracking (customs clearance metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling InternationalShippingClearedFromCustomsEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
