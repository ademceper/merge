using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class FaqCreatedEventHandler(ILogger<FaqCreatedEventHandler> logger) : INotificationHandler<FaqCreatedEvent>
{

    public async Task Handle(FaqCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "FAQ created event received. FaqId: {FaqId}, Question: {Question}, Category: {Category}",
            notification.FaqId, notification.Question, notification.Category);

        // Cache invalidation
        // await _cacheService.RemoveByTagAsync("faqs");

        // Analytics tracking
        // await _analyticsService.TrackFaqCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
