using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Popular Search Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PopularSearchUpdatedEventHandler : INotificationHandler<PopularSearchUpdatedEvent>
{
    private readonly ILogger<PopularSearchUpdatedEventHandler> _logger;

    public PopularSearchUpdatedEventHandler(ILogger<PopularSearchUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PopularSearchUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Popular search updated event received. PopularSearchId: {PopularSearchId}, SearchTerm: {SearchTerm}, SearchCount: {SearchCount}, ClickThroughCount: {ClickThroughCount}, ClickThroughRate: {ClickThroughRate}",
            notification.PopularSearchId, notification.SearchTerm, notification.SearchCount, notification.ClickThroughCount, notification.ClickThroughRate);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (popular searches cache, search suggestions cache)
            // - Analytics tracking (popular search metrics)
            // - Search suggestions update (if search count or click-through rate changed significantly)
            // - External system integration (search analytics service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling PopularSearchUpdatedEvent. PopularSearchId: {PopularSearchId}, SearchTerm: {SearchTerm}",
                notification.PopularSearchId, notification.SearchTerm);
            throw;
        }
    }
}
