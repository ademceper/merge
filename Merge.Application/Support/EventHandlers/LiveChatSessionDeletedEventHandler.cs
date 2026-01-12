using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Live Chat Session Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LiveChatSessionDeletedEventHandler : INotificationHandler<LiveChatSessionDeletedEvent>
{
    private readonly ILogger<LiveChatSessionDeletedEventHandler> _logger;

    public LiveChatSessionDeletedEventHandler(ILogger<LiveChatSessionDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LiveChatSessionDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Live chat session deleted event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, UserId: {UserId}, AgentId: {AgentId}",
            notification.SessionId, notification.SessionIdentifier, notification.UserId, notification.AgentId);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatSessionDeletedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
