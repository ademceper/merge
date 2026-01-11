using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;

namespace Merge.Application.Order.Queries.FilterOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record FilterOrdersQuery(
    Guid? UserId = null,
    string? Status = null,
    string? PaymentStatus = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? OrderNumber = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = true
) : IRequest<PagedResult<OrderDto>>;
