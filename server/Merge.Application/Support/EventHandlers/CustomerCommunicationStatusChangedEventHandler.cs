using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class CustomerCommunicationStatusChangedEventHandler(ILogger<CustomerCommunicationStatusChangedEventHandler> logger) : INotificationHandler<CustomerCommunicationStatusChangedEvent>
{

    public async Task Handle(CustomerCommunicationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Customer communication status changed event received. CommunicationId: {CommunicationId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.CommunicationId, notification.OldStatus, notification.NewStatus);

        // Analytics tracking
        // await _analyticsService.TrackCustomerCommunicationStatusChangedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
