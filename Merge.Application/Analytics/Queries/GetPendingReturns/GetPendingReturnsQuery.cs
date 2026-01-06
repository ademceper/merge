using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Analytics.Queries.GetPendingReturns;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPendingReturnsQuery(
    int Page = 1,
    int PageSize = 0
) : IRequest<PagedResult<ReturnRequestDto>>;

