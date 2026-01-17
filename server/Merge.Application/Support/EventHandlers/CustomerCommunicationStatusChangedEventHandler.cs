using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Customer Communication Status Changed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CustomerCommunicationStatusChangedEventHandler(ILogger<CustomerCommunicationStatusChangedEventHandler> logger) : INotificationHandler<CustomerCommunicationStatusChangedEvent>
{

    public async Task Handle(CustomerCommunicationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Customer communication status changed event received. CommunicationId: {CommunicationId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.CommunicationId, notification.OldStatus, notification.NewStatus);

        // Analytics tracking
        // await _analyticsService.TrackCustomerCommunicationStatusChangedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
