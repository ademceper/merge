using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class InternationalShippingUpdatedEventHandler(ILogger<InternationalShippingUpdatedEventHandler> logger) : INotificationHandler<InternationalShippingUpdatedEvent>
{
    public async Task Handle(InternationalShippingUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "International shipping updated event received. ShippingId: {ShippingId}, OrderId: {OrderId}, UpdateType: {UpdateType}",
            notification.InternationalShippingId, notification.OrderId, notification.UpdateType);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo detayları güncellendi bildirimi)
            // - Analytics tracking (shipping update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling InternationalShippingUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
