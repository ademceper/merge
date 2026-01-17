using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Live Chat Session Assigned Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LiveChatSessionAssignedEventHandler(ILogger<LiveChatSessionAssignedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<LiveChatSessionAssignedEvent>
{
    
    private readonly INotificationService? _notificationService;

    public async Task Handle(LiveChatSessionAssignedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live chat session assigned event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, AgentId: {AgentId}",
            notification.SessionId, notification.SessionIdentifier, notification.AgentId);

        try
        {
            // Agent'a bildirim gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.AgentId,
                    NotificationType.Support,
                    "Yeni Canlı Sohbet Atandı",
                    $"Size yeni bir canlı sohbet atandı. Oturum: {notification.SessionIdentifier}",
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackLiveChatSessionAssignedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveChatSessionAssignedEvent. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, AgentId: {AgentId}",
                notification.SessionId, notification.SessionIdentifier, notification.AgentId);
            throw;
        }
    }
}
