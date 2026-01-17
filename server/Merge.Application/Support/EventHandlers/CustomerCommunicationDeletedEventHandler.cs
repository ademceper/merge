using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Customer Communication Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CustomerCommunicationDeletedEventHandler(ILogger<CustomerCommunicationDeletedEventHandler> logger) : INotificationHandler<CustomerCommunicationDeletedEvent>
{

    public async Task Handle(CustomerCommunicationDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Customer communication deleted event received. CommunicationId: {CommunicationId}, UserId: {UserId}, CommunicationType: {CommunicationType}, Channel: {Channel}",
            notification.CommunicationId, notification.UserId, notification.CommunicationType, notification.Channel);

        // Analytics tracking
        // await _analyticsService.TrackCustomerCommunicationDeletedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
