using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class SearchRecordedEventHandler(
    ILogger<SearchRecordedEventHandler> logger) : INotificationHandler<SearchRecordedEvent>
{

    public async Task Handle(SearchRecordedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling SearchRecordedEvent. SearchHistoryId: {SearchHistoryId}, SearchTerm: {SearchTerm}",
                notification.SearchHistoryId, notification.SearchTerm);
            throw;
        }
    }
}
