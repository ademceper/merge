using MediatR;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Notification.Commands.CreateTemplate;

/// <summary>
/// Create Template Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record CreateTemplateCommand(CreateNotificationTemplateDto Dto) : IRequest<NotificationTemplateDto>;
