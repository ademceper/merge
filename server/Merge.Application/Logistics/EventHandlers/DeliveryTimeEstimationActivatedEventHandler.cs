using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class DeliveryTimeEstimationActivatedEventHandler(
    ILogger<DeliveryTimeEstimationActivatedEventHandler> logger) : INotificationHandler<DeliveryTimeEstimationActivatedEvent>
{

    public async Task Handle(DeliveryTimeEstimationActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "DeliveryTimeEstimation activated event received. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
            notification.DeliveryTimeEstimationId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active delivery time estimations cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling DeliveryTimeEstimationActivatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
