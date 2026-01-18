using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class CustomerCommunicationCreatedEventHandler(ILogger<CustomerCommunicationCreatedEventHandler> logger) : INotificationHandler<CustomerCommunicationCreatedEvent>
{

    public async Task Handle(CustomerCommunicationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Customer communication created event received. CommunicationId: {CommunicationId}, UserId: {UserId}, CommunicationType: {CommunicationType}, Channel: {Channel}, Direction: {Direction}",
            notification.CommunicationId, notification.UserId, notification.CommunicationType, notification.Channel, notification.Direction);

        // Analytics tracking
        // await _analyticsService.TrackCustomerCommunicationCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
