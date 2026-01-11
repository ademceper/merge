using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;

namespace Merge.Application.Order.Queries.GetAllReturnRequests;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllReturnRequestsQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ReturnRequestDto>>;
