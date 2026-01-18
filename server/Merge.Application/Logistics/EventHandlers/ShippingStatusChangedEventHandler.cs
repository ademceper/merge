using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingStatusChangedEventHandler(
    ILogger<ShippingStatusChangedEventHandler> logger) : INotificationHandler<ShippingStatusChangedEvent>
{

    public async Task Handle(ShippingStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Shipping status changed event received. ShippingId: {ShippingId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.ShippingId, notification.OrderId, notification.OldStatus, notification.NewStatus);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo durumu değişti bildirimi)
            // - Email notification (status-specific emails: shipped, delivered, etc.)
            // - Analytics tracking (shipping status transition metrics)
            // - External shipping provider webhook (status update to provider)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingStatusChangedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                notification.ShippingId, notification.OrderId, notification.OldStatus, notification.NewStatus);
            throw;
        }
    }
}
