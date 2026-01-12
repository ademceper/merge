using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Live Chat Session Closed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LiveChatSessionClosedEventHandler : INotificationHandler<LiveChatSessionClosedEvent>
{
    private readonly ILogger<LiveChatSessionClosedEventHandler> _logger;

    public LiveChatSessionClosedEventHandler(ILogger<LiveChatSessionClosedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LiveChatSessionClosedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Live chat session closed event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, UserId: {UserId}, ClosedAt: {ClosedAt}",
            notification.SessionId, notification.SessionIdentifier, notification.UserId, notification.ClosedAt);

        // Analytics tracking
        // await _analyticsService.TrackLiveChatSessionClosedAsync(notification, cancellationToken);

        await Task.CompletedTask;
    }
}
