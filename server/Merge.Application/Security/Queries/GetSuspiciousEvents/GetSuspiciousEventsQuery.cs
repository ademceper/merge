using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Security.Queries.GetSuspiciousEvents;

public record GetSuspiciousEventsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AccountSecurityEventDto>>;
