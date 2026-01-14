using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByProductId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetStockMovementsByProductIdQuery(
    Guid ProductId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<StockMovementDto>>;

