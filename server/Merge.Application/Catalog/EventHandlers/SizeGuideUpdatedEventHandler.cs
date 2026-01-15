using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Size Guide Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SizeGuideUpdatedEventHandler(
    ILogger<SizeGuideUpdatedEventHandler> logger) : INotificationHandler<SizeGuideUpdatedEvent>
{

    public async Task Handle(SizeGuideUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Size guide updated event received. SizeGuideId: {SizeGuideId}, Name: {Name}",
            notification.SizeGuideId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (size guides cache)
            // - Analytics tracking (size guide update metrics)
            // - External system integration
            // - Product size guide cache invalidation

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling SizeGuideUpdatedEvent. SizeGuideId: {SizeGuideId}, Name: {Name}",
                notification.SizeGuideId, notification.Name);
            throw;
        }
    }
}
