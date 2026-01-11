using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Queries.GetPreference;

/// <summary>
/// Get Preference Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record GetPreferenceQuery(
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IRequest<NotificationPreferenceDto?>;
