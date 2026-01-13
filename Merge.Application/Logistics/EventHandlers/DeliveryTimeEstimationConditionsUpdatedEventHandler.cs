using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// DeliveryTimeEstimation Conditions Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class DeliveryTimeEstimationConditionsUpdatedEventHandler : INotificationHandler<DeliveryTimeEstimationConditionsUpdatedEvent>
{
    private readonly ILogger<DeliveryTimeEstimationConditionsUpdatedEventHandler> _logger;

    public DeliveryTimeEstimationConditionsUpdatedEventHandler(ILogger<DeliveryTimeEstimationConditionsUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(DeliveryTimeEstimationConditionsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling DeliveryTimeEstimationConditionsUpdatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
