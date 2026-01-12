using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Customer Communication Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CustomerCommunicationCreatedEventHandler : INotificationHandler<CustomerCommunicationCreatedEvent>
{
    private readonly ILogger<CustomerCommunicationCreatedEventHandler> _logger;

    public CustomerCommunicationCreatedEventHandler(ILogger<CustomerCommunicationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CustomerCommunicationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Customer communication created event received. CommunicationId: {CommunicationId}, UserId: {UserId}, CommunicationType: {CommunicationType}, Channel: {Channel}, Direction: {Direction}",
            notification.CommunicationId, notification.UserId, notification.CommunicationType, notification.Channel, notification.Direction);

        // Analytics tracking
        // await _analyticsService.TrackCustomerCommunicationCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
