using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetNotificationById;


public record GetNotificationByIdQuery(Guid NotificationId) : IRequest<NotificationDto?>;
