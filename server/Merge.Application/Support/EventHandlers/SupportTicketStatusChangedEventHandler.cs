using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class SupportTicketStatusChangedEventHandler(ILogger<SupportTicketStatusChangedEventHandler> logger) : INotificationHandler<SupportTicketStatusChangedEvent>
{

    public async Task Handle(SupportTicketStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Support ticket status changed event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.TicketId, notification.TicketNumber, notification.OldStatus, notification.NewStatus);

        // Analytics tracking
        // await _analyticsService.TrackSupportTicketStatusChangedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
