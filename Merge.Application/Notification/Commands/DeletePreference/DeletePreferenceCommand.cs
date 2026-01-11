using MediatR;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Commands.DeletePreference;

/// <summary>
/// Delete Preference Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record DeletePreferenceCommand(
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IRequest<bool>;
