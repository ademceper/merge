using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Commands.UpdatePreference;

/// <summary>
/// Update Preference Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record UpdatePreferenceCommand(
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel,
    UpdateNotificationPreferenceDto Dto) : IRequest<NotificationPreferenceDto>;
