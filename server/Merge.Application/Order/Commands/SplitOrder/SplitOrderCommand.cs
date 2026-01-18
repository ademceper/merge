using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.SplitOrder;

public record SplitOrderCommand(
    Guid OrderId,
    CreateOrderSplitDto Dto
) : IRequest<OrderSplitDto>;
