using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// DeliveryTimeEstimation Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class DeliveryTimeEstimationDeactivatedEventHandler : INotificationHandler<DeliveryTimeEstimationDeactivatedEvent>
{
    private readonly ILogger<DeliveryTimeEstimationDeactivatedEventHandler> _logger;

    public DeliveryTimeEstimationDeactivatedEventHandler(ILogger<DeliveryTimeEstimationDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(DeliveryTimeEstimationDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling DeliveryTimeEstimationDeactivatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
