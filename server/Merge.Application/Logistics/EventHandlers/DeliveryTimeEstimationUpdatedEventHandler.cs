using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class DeliveryTimeEstimationUpdatedEventHandler(
    ILogger<DeliveryTimeEstimationUpdatedEventHandler> logger) : INotificationHandler<DeliveryTimeEstimationUpdatedEvent>
{

    public async Task Handle(DeliveryTimeEstimationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "DeliveryTimeEstimation updated event received. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}, MinDays: {MinDays}, MaxDays: {MaxDays}, AverageDays: {AverageDays}",
            notification.DeliveryTimeEstimationId, notification.MinDays, notification.MaxDays, notification.AverageDays);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (delivery time estimation cache)
            // - Analytics tracking (estimation update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling DeliveryTimeEstimationUpdatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
