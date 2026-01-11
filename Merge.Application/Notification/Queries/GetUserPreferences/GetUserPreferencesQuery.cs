using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Notification.Queries.GetUserPreferences;

/// <summary>
/// Get User Preferences Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// BOLUM 3.4: Pagination (ZORUNLU)
/// </summary>
public record GetUserPreferencesQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationPreferenceDto>>;
