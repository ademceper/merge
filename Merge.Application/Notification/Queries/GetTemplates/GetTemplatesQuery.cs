using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Queries.GetTemplates;

/// <summary>
/// Get Templates Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// BOLUM 3.4: Pagination (ZORUNLU)
/// </summary>
public record GetTemplatesQuery(
    NotificationType? Type = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationTemplateDto>>;
