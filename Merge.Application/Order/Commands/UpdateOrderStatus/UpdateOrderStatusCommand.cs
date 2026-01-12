using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<bool>;
