using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Search Click Recorded Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SearchClickRecordedEventHandler(
    ILogger<SearchClickRecordedEventHandler> logger) : INotificationHandler<SearchClickRecordedEvent>
{

    public async Task Handle(SearchClickRecordedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Search click recorded event received. SearchHistoryId: {SearchHistoryId}, ProductId: {ProductId}, UserId: {UserId}, SearchTerm: {SearchTerm}",
            notification.SearchHistoryId, notification.ProductId, notification.UserId, notification.SearchTerm);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (search click metrics, user behavior)
            // - Popular search update (increment click-through count)
            // - Search suggestions update (click-through rate calculation)
            // - A/B testing data collection
            // - External system integration (search analytics service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling SearchClickRecordedEvent. SearchHistoryId: {SearchHistoryId}, ProductId: {ProductId}",
                notification.SearchHistoryId, notification.ProductId);
            throw;
        }
    }
}
