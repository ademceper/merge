using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Size Guide Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SizeGuideCreatedEventHandler : INotificationHandler<SizeGuideCreatedEvent>
{
    private readonly ILogger<SizeGuideCreatedEventHandler> _logger;

    public SizeGuideCreatedEventHandler(ILogger<SizeGuideCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SizeGuideCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Size guide created event received. SizeGuideId: {SizeGuideId}, Name: {Name}, CategoryId: {CategoryId}",
            notification.SizeGuideId, notification.Name, notification.CategoryId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (size guides cache)
            // - Analytics tracking
            // - External system integration

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SizeGuideCreatedEvent. SizeGuideId: {SizeGuideId}, Name: {Name}",
                notification.SizeGuideId, notification.Name);
            throw;
        }
    }
}
