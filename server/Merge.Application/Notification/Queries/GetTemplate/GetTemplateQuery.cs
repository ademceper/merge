using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplate;

/// <summary>
/// Get Template Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record GetTemplateQuery(Guid Id) : IRequest<NotificationTemplateDto?>;
