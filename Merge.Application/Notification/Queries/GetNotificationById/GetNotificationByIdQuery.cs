using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetNotificationById;

/// <summary>
/// Get Notification By Id Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record GetNotificationByIdQuery(Guid NotificationId) : IRequest<NotificationDto?>;
