using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class DeliveryTimeEstimationConditionsUpdatedEventHandler(
    ILogger<DeliveryTimeEstimationConditionsUpdatedEventHandler> logger) : INotificationHandler<DeliveryTimeEstimationConditionsUpdatedEvent>
{

    public async Task Handle(DeliveryTimeEstimationConditionsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "DeliveryTimeEstimation conditions updated event received. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
            notification.DeliveryTimeEstimationId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (delivery time estimation cache)
            // - Analytics tracking (estimation conditions update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling DeliveryTimeEstimationConditionsUpdatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
