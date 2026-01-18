using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreateTemplate;


public record CreateTemplateCommand(CreateNotificationTemplateDto Dto) : IRequest<NotificationTemplateDto>;
