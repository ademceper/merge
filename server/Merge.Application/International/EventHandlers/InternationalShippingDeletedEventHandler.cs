using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class InternationalShippingDeletedEventHandler(ILogger<InternationalShippingDeletedEventHandler> logger) : INotificationHandler<InternationalShippingDeletedEvent>
{
    public async Task Handle(InternationalShippingDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "International shipping deleted event received. ShippingId: {ShippingId}, OrderId: {OrderId}",
            notification.InternationalShippingId, notification.OrderId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (shipping deletion metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling InternationalShippingDeletedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
