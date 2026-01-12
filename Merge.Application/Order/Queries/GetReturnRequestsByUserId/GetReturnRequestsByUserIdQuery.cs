using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetReturnRequestsByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReturnRequestsByUserIdQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ReturnRequestDto>>;
