using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetPreference;


public record GetPreferenceQuery(
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IRequest<NotificationPreferenceDto?>;
