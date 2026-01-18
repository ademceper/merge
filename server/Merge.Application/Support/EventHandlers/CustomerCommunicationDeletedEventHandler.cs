using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class CustomerCommunicationDeletedEventHandler(ILogger<CustomerCommunicationDeletedEventHandler> logger) : INotificationHandler<CustomerCommunicationDeletedEvent>
{

    public async Task Handle(CustomerCommunicationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Customer communication deleted event received. CommunicationId: {CommunicationId}, UserId: {UserId}, CommunicationType: {CommunicationType}, Channel: {Channel}",
            notification.CommunicationId, notification.UserId, notification.CommunicationType, notification.Channel);

        // Analytics tracking
        // await _analyticsService.TrackCustomerCommunicationDeletedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
