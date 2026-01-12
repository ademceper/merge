using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Live Chat Message Sent Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LiveChatMessageSentEventHandler : INotificationHandler<LiveChatMessageSentEvent>
{
    private readonly ILogger<LiveChatMessageSentEventHandler> _logger;

    public LiveChatMessageSentEventHandler(ILogger<LiveChatMessageSentEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LiveChatMessageSentEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Live chat message sent event received. MessageId: {MessageId}, SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, SenderId: {SenderId}, SenderType: {SenderType}",
            notification.MessageId, notification.SessionId, notification.SessionIdentifier, notification.SenderId, notification.SenderType);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatMessageSentAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
