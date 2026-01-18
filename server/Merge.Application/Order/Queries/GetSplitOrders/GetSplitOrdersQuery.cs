using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetSplitOrders;

public record GetSplitOrdersQuery(
    Guid SplitOrderId
) : IRequest<IEnumerable<OrderSplitDto>>;
