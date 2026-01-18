using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Security.Queries.GetUserSecurityEvents;

public record GetUserSecurityEventsQuery(
    Guid UserId,
    string? EventType = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AccountSecurityEventDto>>;
