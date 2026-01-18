using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreatePreference;


public record CreatePreferenceCommand(
    Guid UserId,
    CreateNotificationPreferenceDto Dto) : IRequest<NotificationPreferenceDto>;
