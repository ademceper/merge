using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Size Guide Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SizeGuideDeletedEventHandler : INotificationHandler<SizeGuideDeletedEvent>
{
    private readonly ILogger<SizeGuideDeletedEventHandler> _logger;

    public SizeGuideDeletedEventHandler(ILogger<SizeGuideDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SizeGuideDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Size guide deleted event received. SizeGuideId: {SizeGuideId}, Name: {Name}",
            notification.SizeGuideId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (size guides cache)
            // - Analytics tracking (size guide deletion metrics)
            // - External system integration
            // - Cascade delete handling (size guide entries, product size guides)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SizeGuideDeletedEvent. SizeGuideId: {SizeGuideId}, Name: {Name}",
                notification.SizeGuideId, notification.Name);
            throw;
        }
    }
}
