using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class DeliveryTimeEstimationCreatedEventHandler(
    ILogger<DeliveryTimeEstimationCreatedEventHandler> logger) : INotificationHandler<DeliveryTimeEstimationCreatedEvent>
{

    public async Task Handle(DeliveryTimeEstimationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "DeliveryTimeEstimation created event received. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}, ProductId: {ProductId}, CategoryId: {CategoryId}, WarehouseId: {WarehouseId}, MinDays: {MinDays}, MaxDays: {MaxDays}, AverageDays: {AverageDays}",
            notification.DeliveryTimeEstimationId, notification.ProductId, notification.CategoryId, notification.WarehouseId, notification.MinDays, notification.MaxDays, notification.AverageDays);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (delivery time estimation cache)
            // - Analytics tracking (estimation creation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling DeliveryTimeEstimationCreatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
