using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class PopularSearchCreatedEventHandler(
    ILogger<PopularSearchCreatedEventHandler> logger) : INotificationHandler<PopularSearchCreatedEvent>
{

    public async Task Handle(PopularSearchCreatedEvent notification, CancellationToken cancellationToken)
    {
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
            logger.LogError(ex,
                "Error handling PopularSearchCreatedEvent. PopularSearchId: {PopularSearchId}, SearchTerm: {SearchTerm}",
                notification.PopularSearchId, notification.SearchTerm);
            throw;
        }
    }
}
