using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class PurchaseOrderItemAddedEventHandler(
    ILogger<PurchaseOrderItemAddedEventHandler> logger) : INotificationHandler<PurchaseOrderItemAddedEvent>
{

    public async Task Handle(PurchaseOrderItemAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Purchase order item added event received. PurchaseOrderId: {PurchaseOrderId}, ProductId: {ProductId}, Quantity: {Quantity}, UnitPrice: {UnitPrice}",
            notification.PurchaseOrderId, notification.ProductId, notification.Quantity, notification.UnitPrice);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (purchase order cache)
        // - Analytics tracking
        // - Real-time updates (SignalR)

        await Task.CompletedTask;
    }
}
