using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class DeliveryTimeEstimationDeactivatedEventHandler(
    ILogger<DeliveryTimeEstimationDeactivatedEventHandler> logger) : INotificationHandler<DeliveryTimeEstimationDeactivatedEvent>
{

    public async Task Handle(DeliveryTimeEstimationDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "DeliveryTimeEstimation deactivated event received. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
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
                "Error handling DeliveryTimeEstimationDeactivatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
