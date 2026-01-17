using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Support Ticket Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SupportTicketDeletedEventHandler(ILogger<SupportTicketDeletedEventHandler> logger) : INotificationHandler<SupportTicketDeletedEvent>
{

    public async Task Handle(SupportTicketDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
