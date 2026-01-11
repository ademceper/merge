using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Queries.GetOrderSplit;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetOrderSplitQuery(
    Guid SplitId
) : IRequest<OrderSplitDto?>;
