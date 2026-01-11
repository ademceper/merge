using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Queries.GetTemplateByType;

/// <summary>
/// Get Template By Type Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record GetTemplateByTypeQuery(NotificationType Type) : IRequest<NotificationTemplateDto?>;
