using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetB2BUserPurchaseOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetB2BUserPurchaseOrdersQuery(
    Guid B2BUserId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PurchaseOrderDto>>;

