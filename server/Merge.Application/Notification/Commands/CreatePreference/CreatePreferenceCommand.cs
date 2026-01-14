using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreatePreference;

/// <summary>
/// Create Preference Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record CreatePreferenceCommand(
    Guid UserId,
    CreateNotificationPreferenceDto Dto) : IRequest<NotificationPreferenceDto>;
