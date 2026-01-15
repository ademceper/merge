using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class InternationalShippingStatusChangedEventHandler(ILogger<InternationalShippingStatusChangedEventHandler> logger) : INotificationHandler<InternationalShippingStatusChangedEvent>
{
    public async Task Handle(InternationalShippingStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "International shipping status changed event received. ShippingId: {ShippingId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.InternationalShippingId, notification.OrderId, notification.OldStatus, notification.NewStatus);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo durumu değişikliği bildirimi)
            // - Analytics tracking (shipping status change metrics)
            // - External shipping provider status sync

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling InternationalShippingStatusChangedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
