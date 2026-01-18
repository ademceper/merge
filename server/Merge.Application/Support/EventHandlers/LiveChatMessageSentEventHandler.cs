using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class LiveChatMessageSentEventHandler(ILogger<LiveChatMessageSentEventHandler> logger) : INotificationHandler<LiveChatMessageSentEvent>
{

    public async Task Handle(LiveChatMessageSentEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live chat message sent event received. MessageId: {MessageId}, SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, SenderId: {SenderId}, SenderType: {SenderType}",
            notification.MessageId, notification.SessionId, notification.SessionIdentifier, notification.SenderId, notification.SenderType);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatMessageSentAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
