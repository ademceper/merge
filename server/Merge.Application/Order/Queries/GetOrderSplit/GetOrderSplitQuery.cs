using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetOrderSplit;

public record GetOrderSplitQuery(
    Guid SplitId
) : IRequest<OrderSplitDto?>;
