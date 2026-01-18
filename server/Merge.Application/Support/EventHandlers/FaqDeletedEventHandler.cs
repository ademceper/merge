using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class FaqDeletedEventHandler(ILogger<FaqDeletedEventHandler> logger) : INotificationHandler<FaqDeletedEvent>
{

    public async Task Handle(FaqDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "FAQ deleted event received. FaqId: {FaqId}, Question: {Question}, Category: {Category}",
            notification.FaqId, notification.Question, notification.Category);

        // Analytics tracking
        // await _analyticsService.TrackFaqDeletedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
