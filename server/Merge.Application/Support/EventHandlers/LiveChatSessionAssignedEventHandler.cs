using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class LiveChatSessionAssignedEventHandler(ILogger<LiveChatSessionAssignedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<LiveChatSessionAssignedEvent>
{
    public async Task Handle(LiveChatSessionAssignedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live chat session assigned event received. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, AgentId: {AgentId}",
            notification.SessionId, notification.SessionIdentifier, notification.AgentId);

        try
        {
            // Agent'a bildirim gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.AgentId,
                    NotificationType.Support,
                    "Yeni Canlı Sohbet Atandı",
                    $"Size yeni bir canlı sohbet atandı. Oturum: {notification.SessionIdentifier}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackLiveChatSessionAssignedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveChatSessionAssignedEvent. SessionId: {SessionId}, SessionIdentifier: {SessionIdentifier}, AgentId: {AgentId}",
                notification.SessionId, notification.SessionIdentifier, notification.AgentId);
            throw;
        }
    }
}
