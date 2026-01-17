using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Live Chat Session Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LiveChatSessionCreatedEventHandler(ILogger<LiveChatSessionCreatedEventHandler> logger) : INotificationHandler<LiveChatSessionCreatedEvent>
{

    public async Task Handle(LiveChatSessionCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live chat session created event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, UserId: {UserId}, GuestName: {GuestName}, Department: {Department}",
            notification.SessionId, notification.SessionIdentifier, notification.UserId, notification.GuestName, notification.Department);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatSessionCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
