using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class LiveChatSessionDeletedEventHandler(ILogger<LiveChatSessionDeletedEventHandler> logger) : INotificationHandler<LiveChatSessionDeletedEvent>
{

    public async Task Handle(LiveChatSessionDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live chat session deleted event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, UserId: {UserId}, AgentId: {AgentId}",
            notification.SessionId, notification.SessionIdentifier, notification.UserId, notification.AgentId);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatSessionDeletedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
