using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Popular Search Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PopularSearchCreatedEventHandler(
    ILogger<PopularSearchCreatedEventHandler> logger) : INotificationHandler<PopularSearchCreatedEvent>
{

    public async Task Handle(PopularSearchCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Popular search created event received. PopularSearchId: {PopularSearchId}, SearchTerm: {SearchTerm}",
            notification.PopularSearchId, notification.SearchTerm);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (popular searches cache)
            // - Analytics tracking (popular search creation metrics)
            // - Search index update (autocomplete suggestions)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling PopularSearchCreatedEvent. PopularSearchId: {PopularSearchId}, SearchTerm: {SearchTerm}",
                notification.PopularSearchId, notification.SearchTerm);
            throw;
        }
    }
}
