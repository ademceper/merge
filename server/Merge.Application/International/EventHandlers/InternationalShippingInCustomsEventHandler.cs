using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class InternationalShippingInCustomsEventHandler(ILogger<InternationalShippingInCustomsEventHandler> logger) : INotificationHandler<InternationalShippingInCustomsEvent>
{
    public async Task Handle(InternationalShippingInCustomsEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "International shipping in customs event received. ShippingId: {ShippingId}, OrderId: {OrderId}",
            notification.InternationalShippingId, notification.OrderId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo gümrükte bildirimi)
            // - Analytics tracking (customs processing metrics)
            // - Customs documentation tracking

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling InternationalShippingInCustomsEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
