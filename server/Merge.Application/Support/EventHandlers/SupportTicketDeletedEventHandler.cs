using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class SupportTicketDeletedEventHandler(ILogger<SupportTicketDeletedEventHandler> logger) : INotificationHandler<SupportTicketDeletedEvent>
{

    public async Task Handle(SupportTicketDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Support ticket deleted event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}",
            notification.TicketId, notification.TicketNumber, notification.UserId);

        // Analytics tracking
        // await _analyticsService.TrackSupportTicketDeletedAsync(notification, cancellationToken);

        // Cleanup related data if needed
        // await _cleanupService.CleanupTicketRelatedDataAsync(notification.TicketId, cancellationToken);

        await Task.CompletedTask;
    }
}
