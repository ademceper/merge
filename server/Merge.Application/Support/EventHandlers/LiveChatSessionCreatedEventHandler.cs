using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class LiveChatSessionCreatedEventHandler(ILogger<LiveChatSessionCreatedEventHandler> logger) : INotificationHandler<LiveChatSessionCreatedEvent>
{

    public async Task Handle(LiveChatSessionCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live chat session created event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, UserId: {UserId}, GuestName: {GuestName}, Department: {Department}",
            notification.SessionId, notification.SessionIdentifier, notification.UserId, notification.GuestName, notification.Department);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatSessionCreatedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
