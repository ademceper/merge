using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.Reorder;

public record ReorderCommand(
    Guid OrderId,
    Guid UserId
) : IRequest<OrderDto>;
