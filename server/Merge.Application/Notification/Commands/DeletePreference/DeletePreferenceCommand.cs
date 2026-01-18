using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.DeletePreference;


public record DeletePreferenceCommand(
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IRequest<bool>;
