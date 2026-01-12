using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Support Ticket Status Changed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SupportTicketStatusChangedEventHandler : INotificationHandler<SupportTicketStatusChangedEvent>
{
    private readonly ILogger<SupportTicketStatusChangedEventHandler> _logger;

    public SupportTicketStatusChangedEventHandler(ILogger<SupportTicketStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SupportTicketStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Support ticket status changed event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.TicketId, notification.TicketNumber, notification.OldStatus, notification.NewStatus);

        // Analytics tracking
        // await _analyticsService.TrackSupportTicketStatusChangedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
