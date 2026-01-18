using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUserPreferences;


public record GetUserPreferencesQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationPreferenceDto>>;
