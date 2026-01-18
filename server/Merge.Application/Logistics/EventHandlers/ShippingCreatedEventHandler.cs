using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingCreatedEventHandler(
    ILogger<ShippingCreatedEventHandler> logger) : INotificationHandler<ShippingCreatedEvent>
{

    public async Task Handle(ShippingCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Shipping created event received. ShippingId: {ShippingId}, OrderId: {OrderId}, ShippingProvider: {ShippingProvider}, ShippingCost: {ShippingCost}",
            notification.ShippingId, notification.OrderId, notification.ShippingProvider, notification.ShippingCost);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo oluşturuldu bildirimi)
            // - Analytics tracking (shipping creation metrics)
            // - External shipping provider integration (tracking number generation)
            // - Email notification (order shipped email)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingCreatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, ShippingProvider: {ShippingProvider}",
                notification.ShippingId, notification.OrderId, notification.ShippingProvider);
            throw;
        }
    }
}
