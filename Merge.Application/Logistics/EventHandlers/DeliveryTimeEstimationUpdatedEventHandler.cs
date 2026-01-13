using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// DeliveryTimeEstimation Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class DeliveryTimeEstimationUpdatedEventHandler : INotificationHandler<DeliveryTimeEstimationUpdatedEvent>
{
    private readonly ILogger<DeliveryTimeEstimationUpdatedEventHandler> _logger;

    public DeliveryTimeEstimationUpdatedEventHandler(ILogger<DeliveryTimeEstimationUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(DeliveryTimeEstimationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling DeliveryTimeEstimationUpdatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
