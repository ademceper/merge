using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetOrderSplits;

public record GetOrderSplitsQuery(
    Guid OrderId
) : IRequest<IEnumerable<OrderSplitDto>>;
