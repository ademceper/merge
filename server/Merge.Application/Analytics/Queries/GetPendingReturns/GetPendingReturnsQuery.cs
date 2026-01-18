using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Analytics.Queries.GetPendingReturns;

public record GetPendingReturnsQuery(
    int Page = 1,
    int PageSize = 0
) : IRequest<PagedResult<ReturnRequestDto>>;

