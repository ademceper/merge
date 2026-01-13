using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// PickPack Details Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PickPackDetailsUpdatedEventHandler : INotificationHandler<PickPackDetailsUpdatedEvent>
{
    private readonly ILogger<PickPackDetailsUpdatedEventHandler> _logger;

    public PickPackDetailsUpdatedEventHandler(ILogger<PickPackDetailsUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PickPackDetailsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "PickPack details updated event received. PickPackId: {PickPackId}, OrderId: {OrderId}",
            notification.PickPackId, notification.OrderId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (pick-pack details cache)
            // - Analytics tracking (pick-pack update metrics)
            // - Notification gönderimi (warehouse staff'a pick-pack güncelleme bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling PickPackDetailsUpdatedEvent. PickPackId: {PickPackId}, OrderId: {OrderId}",
                notification.PickPackId, notification.OrderId);
            throw;
        }
    }
}
