using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class LiveChatSessionClosedEventHandler(ILogger<LiveChatSessionClosedEventHandler> logger) : INotificationHandler<LiveChatSessionClosedEvent>
{

    public async Task Handle(LiveChatSessionClosedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live chat session closed event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, UserId: {UserId}, ClosedAt: {ClosedAt}",
            notification.SessionId, notification.SessionIdentifier, notification.UserId, notification.ClosedAt);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatSessionClosedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
