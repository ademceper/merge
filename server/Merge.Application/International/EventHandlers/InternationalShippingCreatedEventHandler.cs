using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class InternationalShippingCreatedEventHandler(ILogger<InternationalShippingCreatedEventHandler> logger) : INotificationHandler<InternationalShippingCreatedEvent>
{
    public async Task Handle(InternationalShippingCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "International shipping created event received. ShippingId: {ShippingId}, OrderId: {OrderId}, OriginCountry: {OriginCountry}, DestinationCountry: {DestinationCountry}, ShippingMethod: {ShippingMethod}, ShippingCost: {ShippingCost}",
            notification.InternationalShippingId, notification.OrderId, notification.OriginCountry, notification.DestinationCountry, notification.ShippingMethod, notification.ShippingCost);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya uluslararası kargo oluşturuldu bildirimi)
            // - Analytics tracking (international shipping metrics)
            // - Customs documentation generation
            // - External shipping provider integration

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling InternationalShippingCreatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
