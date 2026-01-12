using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Search Recorded Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SearchRecordedEventHandler : INotificationHandler<SearchRecordedEvent>
{
    private readonly ILogger<SearchRecordedEventHandler> _logger;

    public SearchRecordedEventHandler(ILogger<SearchRecordedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SearchRecordedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Search recorded event received. SearchHistoryId: {SearchHistoryId}, UserId: {UserId}, SearchTerm: {SearchTerm}, ResultCount: {ResultCount}",
            notification.SearchHistoryId, notification.UserId, notification.SearchTerm, notification.ResultCount);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (search metrics, user behavior)
            // - Popular search update (increment search count)
            // - Search suggestions update
            // - A/B testing data collection

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SearchRecordedEvent. SearchHistoryId: {SearchHistoryId}, SearchTerm: {SearchTerm}",
                notification.SearchHistoryId, notification.SearchTerm);
            throw;
        }
    }
}
